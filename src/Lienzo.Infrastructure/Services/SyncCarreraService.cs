using Lienzo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Lienzo.Infrastructure.Services;

public class SyncCarreraService : ISyncCarreraService
{
    private readonly string _connectionString;

    public SyncCarreraService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TobaConnection")
            ?? throw new InvalidOperationException("TobaConnection string is not configured");
    }

    public async Task<List<ExternalCarreraInfo>> GetCarrerasAsync()
    {
        var items = new List<ExternalCarreraInfo>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT propuesta, nombre, codigo
              FROM negocio.sga_propuestas
              WHERE estado = 'A'
              ORDER BY nombre",
            conn);

        await using var r = await cmd.ExecuteReaderAsync();

        while (await r.ReadAsync())
        {
            items.Add(new ExternalCarreraInfo
            {
                Propuesta = r.GetInt32(0),
                Nombre = r.IsDBNull(1) ? "" : r.GetString(1).Trim(),
                Codigo = r.IsDBNull(2) ? "" : r.GetString(2).Trim(),
            });
        }

        return items;
    }
}
