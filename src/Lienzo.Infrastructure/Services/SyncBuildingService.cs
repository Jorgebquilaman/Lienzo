using Lienzo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Lienzo.Infrastructure.Services;

public class SyncBuildingService : ISyncBuildingService
{
    private readonly string _connectionString;

    public SyncBuildingService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TobaConnection")
            ?? throw new InvalidOperationException("TobaConnection string is not configured");
    }

    public async Task<List<ExternalBuildingInfo>> GetExternalBuildingsAsync()
    {
        var items = new List<ExternalBuildingInfo>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT edificacion, nombre FROM negocio.sga_edificaciones WHERE estado = 'A' ORDER BY nombre",
            connection);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var nombre = reader.GetString(1);
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                items.Add(new ExternalBuildingInfo
                {
                    Edificacion = reader.GetInt32(0),
                    Nombre = nombre.Trim()
                });
            }
        }

        return items;
    }
}
