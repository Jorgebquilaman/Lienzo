using Lienzo.Application.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Lienzo.Infrastructure.Services;

public class SyncEstudianteService : ISyncEstudianteService
{
    private readonly string _connectionString;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncEstudianteService> _logger;

    public SyncEstudianteService(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<SyncEstudianteService> logger)
    {
        _connectionString = configuration.GetConnectionString("TobaConnection")
            ?? throw new InvalidOperationException("TobaConnection string is not configured");
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<List<ExternalEstudianteInfo>> GetEstudiantesAsync(short anioAcademico)
    {
        var items = new List<ExternalEstudianteInfo>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT DISTINCT a.alumno, p.persona, p.apellido, p.nombres, p.usuario
              FROM negocio.sga_alumnos a
              JOIN negocio.mdp_personas p ON p.persona = a.persona
              WHERE a.estado = 'A'
                AND EXISTS (
                  SELECT 1 FROM negocio.sga_clases_asistencia ca
                  JOIN negocio.sga_clases sc ON sc.clase = ca.clase
                  JOIN negocio.sga_comisiones_bh bh ON bh.banda_horaria = sc.banda_horaria
                  JOIN negocio.sga_comisiones c ON c.comision = bh.comision
                  JOIN negocio.sga_periodos_lectivos pl ON pl.periodo_lectivo = c.periodo_lectivo
                  JOIN negocio.sga_periodos pe ON pe.periodo = pl.periodo
                  WHERE ca.alumno = a.alumno AND pe.anio_academico = @anio
                )
              ORDER BY p.apellido, p.nombres",
            conn);
        cmd.Parameters.AddWithValue("anio", anioAcademico);

        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            items.Add(new ExternalEstudianteInfo
            {
                AlumnoId = r.GetInt32(0),
                PersonaId = r.GetInt32(1),
                Apellido = r.IsDBNull(2) ? "" : r.GetString(2).Trim(),
                Nombres = r.IsDBNull(3) ? "" : r.GetString(3).Trim(),
                Usuario = r.IsDBNull(4) ? "" : r.GetString(4).Trim(),
            });
        }

        await r.CloseAsync();
        return items;
    }

    public async Task<SyncEstudianteResult> CreateMissingEstudianteUsersAsync(short anioAcademico)
    {
        var result = new SyncEstudianteResult();
        var estudiantes = await GetEstudiantesAsync(anioAcademico);

        _logger.LogInformation("Found {Count} estudiantes for year {Year}", estudiantes.Count, anioAcademico);

        foreach (var est in estudiantes)
        {
            try
            {
                var success = await CreateUserIfNeededAsync(est);
                if (success)
                    result.Creados++;
                else
                    result.Existentes++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user for estudiante '{Nombre}'", $"{est.Nombres} {est.Apellido}");
                result.CorreosErroneos.Add(est.Usuario);
            }
        }

        result.Errores = result.CorreosErroneos.Count;
        return result;
    }

    private async Task<bool> CreateUserIfNeededAsync(ExternalEstudianteInfo est)
    {
        if (string.IsNullOrWhiteSpace(est.Usuario))
            return false;

        var email = est.Usuario.Trim().ToLowerInvariant();
        var firstName = Capitalize(est.Nombres);
        var lastName = Capitalize(est.Apellido);

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            return false;

        var existingByPersona = await userManager.Users
            .FirstOrDefaultAsync(u => u.SgaPersonaId == est.PersonaId);
        if (existingByPersona is not null)
            return false;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            SgaPersonaId = est.PersonaId,
        };

        var createResult = await userManager.CreateAsync(user, "Estudiante123!");
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user for {Email}: {Errors}", email, errors);
            return false;
        }

        await userManager.AddToRoleAsync(user, "Student");
        _logger.LogInformation("Created student user '{Name}' with email {Email}", $"{firstName} {lastName}", email);
        return true;
    }

    private static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return string.Join(" ", s.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => char.ToUpper(p[0]) + p[1..].ToLower()));
    }
}
