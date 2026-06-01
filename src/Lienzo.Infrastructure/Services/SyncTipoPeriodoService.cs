using Lienzo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Lienzo.Infrastructure.Services;

public class SyncTipoPeriodoService : ISyncTipoPeriodoService
{
    private readonly string _connectionString;

    public SyncTipoPeriodoService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TobaConnection")
            ?? throw new InvalidOperationException("TobaConnection string is not configured");
    }

    public async Task<List<ExternalTipoPeriodoInfo>> GetTiposPeriodoAsync()
    {
        var items = new List<ExternalTipoPeriodoInfo>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT periodo_generico, nombre FROM negocio.sga_periodos_genericos WHERE activo = 'S' ORDER BY nombre",
            conn);
        await using var r = await cmd.ExecuteReaderAsync();

        while (await r.ReadAsync())
        {
            var nombre = r.GetString(1);
            if (!string.IsNullOrWhiteSpace(nombre))
                items.Add(new ExternalTipoPeriodoInfo { PeriodoGenerico = r.GetInt32(0), Nombre = nombre.Trim() });
        }

        return items;
    }
}
