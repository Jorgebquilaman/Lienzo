using Lienzo.Application.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lienzo.Infrastructure.Services;

public class SyncDocenteService : ISyncDocenteService
{
    private readonly ISyncActividadService _syncService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncDocenteService> _logger;

    public SyncDocenteService(
        ISyncActividadService syncService,
        IServiceScopeFactory scopeFactory,
        ILogger<SyncDocenteService> logger)
    {
        _syncService = syncService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<SyncDocenteResult> CreateMissingDocenteUsersAsync(short anioAcademico)
    {
        var result = new SyncDocenteResult();

        // Get all external actividades with docente names
        var external = await _syncService.GetActividadesAsync(anioAcademico);

        // Collect unique docente names with their emails
        var uniqueDocentes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in external)
        {
            foreach (var name in item.DocenteNames)
            {
                var trimmed = name.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;
                if (!uniqueDocentes.ContainsKey(trimmed))
                {
                    var email = item.DocenteEmails.TryGetValue(trimmed, out var e) ? e : string.Empty;
                    uniqueDocentes[trimmed] = email;
                }
            }
        }

        _logger.LogInformation("Found {Count} unique docente names for year {Year}", uniqueDocentes.Count, anioAcademico);

        foreach (var kvp in uniqueDocentes)
        {
            try
            {
                var success = await CreateUserIfNeededAsync(kvp.Key, kvp.Value);
                if (success)
                    result.Creados++;
                else
                    result.Existentes++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user for docente '{Name}'", kvp.Key);
                result.NombresErroneos.Add(kvp.Key);
            }
        }

        result.Errores = result.NombresErroneos.Count;
        return result;
    }

    private async Task<bool> CreateUserIfNeededAsync(string fullName, string emailFromDb)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return false;

        var lastName = Capitalize(parts[^1]);
        var firstName = string.Join(" ", parts.Take(parts.Length - 1).Select(Capitalize));
        var email = emailFromDb ?? $"{parts[0]}.{parts[^1]}@lienzo.edu".ToLowerInvariant();
        email = NormalizeEmail(email);

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            return false;

        // Check if name already matches any existing user
        var allUsers = await userManager.Users.ToListAsync();
        var matchedUser = allUsers.FirstOrDefault(u =>
            string.Equals($"{u.FirstName} {u.LastName}", fullName, StringComparison.OrdinalIgnoreCase));
        if (matchedUser is not null)
            return false;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, "Docente123!");
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user for {Name} ({Email}): {Errors}", fullName, email, errors);
            return false;
        }

        await userManager.AddToRoleAsync(user, "Teacher");
        _logger.LogInformation("Created user '{Name}' with email {Email} and role Teacher", fullName, email);
        return true;
    }

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];

    private static string NormalizeEmail(string input)
    {
        // Replace accented characters with ASCII equivalents
        var normalized = input
            .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u')
            .Replace('Á', 'a').Replace('É', 'e').Replace('Í', 'i').Replace('Ó', 'o').Replace('Ú', 'u')
            .Replace('ü', 'u').Replace('Ü', 'u')
            .Replace('ñ', 'n').Replace('Ñ', 'n');
        // Remove any remaining non-alphanumeric chars except . and @
        var allowed = new string(normalized.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '@').ToArray());
        return allowed;
    }
}
