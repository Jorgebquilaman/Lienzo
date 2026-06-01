using Lienzo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Lienzo.Infrastructure.Services;

public class SyncPeriodoService : ISyncPeriodoService
{
    private readonly string _connectionString;

    public SyncPeriodoService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TobaConnection")
            ?? throw new InvalidOperationException("TobaConnection string is not configured");
    }

    public async Task<List<ExternalPeriodoInfo>> GetPeriodosAsync(short anioAcademico)
    {
        var items = new List<ExternalPeriodoInfo>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT periodo, nombre, anio_academico, periodo_generico, fecha_inicio, fecha_fin
              FROM negocio.sga_periodos
              WHERE anio_academico = @anio
              ORDER BY nombre",
            conn);
        cmd.Parameters.AddWithValue("anio", anioAcademico);

        await using var r = await cmd.ExecuteReaderAsync();

        while (await r.ReadAsync())
        {
            var nombre = r.GetString(1);
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                items.Add(new ExternalPeriodoInfo
                {
                    Periodo = r.GetInt32(0),
                    Nombre = nombre.Trim(),
                    AnioAcademico = r.GetInt16(2),
                    PeriodoGenerico = r.GetInt32(3),
                    FechaInicio = DateOnly.FromDateTime(r.GetDateTime(4)),
                    FechaFin = DateOnly.FromDateTime(r.GetDateTime(5))
                });
            }
        }

        return items;
    }
}
