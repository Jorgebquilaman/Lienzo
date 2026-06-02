using Lienzo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Lienzo.Infrastructure.Services;

public class SgaAsistenciaService : ISgaAsistenciaService
{
    private readonly string _connectionString;
    private readonly ILogger<SgaAsistenciaService> _logger;

    public SgaAsistenciaService(IConfiguration configuration, ILogger<SgaAsistenciaService> logger)
    {
        _connectionString = configuration.GetConnectionString("TobaConnection")
            ?? throw new InvalidOperationException("TobaConnection string is not configured");
        _logger = logger;
    }

    public async Task<List<SgaClaseAlumnoInfo>> GetAlumnosPorComisionFechaAsync(int comision, DateOnly fecha)
    {
        var items = new List<SgaClaseAlumnoInfo>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT sc.clase, sc.fecha, scb.comision, sa.alumno, mp.persona,
                     mp.apellido, mp.nombres
              FROM negocio.sga_clases sc
              INNER JOIN negocio.sga_comisiones_bh scb ON scb.banda_horaria = sc.banda_horaria
              INNER JOIN negocio.sga_clases_asistencia sca ON sca.clase = sc.clase
              INNER JOIN negocio.sga_alumnos sa ON sa.alumno = sca.alumno
              INNER JOIN negocio.mdp_personas mp ON mp.persona = sa.persona
              WHERE scb.comision = @comision AND sc.fecha = @fecha
              ORDER BY mp.apellido, mp.nombres",
            conn);
        cmd.Parameters.AddWithValue("comision", comision);
        cmd.Parameters.Add(new NpgsqlParameter("fecha", NpgsqlDbType.Date) { Value = fecha });

        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            items.Add(new SgaClaseAlumnoInfo
            {
                SgaClaseId = r.GetInt32(0),
                Fecha = DateOnly.FromDateTime(r.GetDateTime(1)),
                Comision = r.GetInt32(2),
                AlumnoId = r.GetInt32(3),
                PersonaId = r.GetInt32(4),
                Apellido = r.IsDBNull(5) ? "" : r.GetString(5).Trim(),
                Nombres = r.IsDBNull(6) ? "" : r.GetString(6).Trim(),
                SgaAsistenciaId = 0,
            });
        }

        await r.CloseAsync();
        return items;
    }

    public async Task<SyncSgaResult> SincronizarAsistenciaAsync(int sgaClaseId, List<(int AlumnoId, bool Presente)> asistencias)
    {
        var result = new SyncSgaResult();

        if (asistencias.Count == 0)
            return result;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        foreach (var (alumnoId, presente) in asistencias)
        {
            try
            {
                await using var cmd = new NpgsqlCommand(
                    @"UPDATE negocio.sga_clases_asistencia
                      SET presente = @presente,
                          fecha_hora_marcado = @ahora
                      WHERE clase = @sgaClaseId AND alumno = @alumnoId",
                    conn);
                cmd.Parameters.AddWithValue("presente", presente);
                cmd.Parameters.AddWithValue("ahora", DateTime.Now);
                cmd.Parameters.AddWithValue("sgaClaseId", sgaClaseId);
                cmd.Parameters.AddWithValue("alumnoId", alumnoId);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows > 0)
                    result.Actualizados++;
                else
                {
                    result.Errores++;
                    result.DetalleErrores.Add($"No se encontró registro para clase {sgaClaseId}, alumno {alumnoId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar asistencia para clase {SgaClaseId}, alumno {AlumnoId}", sgaClaseId, alumnoId);
                result.Errores++;
                result.DetalleErrores.Add($"Error en clase {sgaClaseId}, alumno {alumnoId}: {ex.Message}");
            }
        }

        return result;
    }
}
