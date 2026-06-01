using Lienzo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Lienzo.Infrastructure.Services;

public class SyncClassroomService : ISyncClassroomService
{
    private readonly string _connectionString;

    public SyncClassroomService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TobaConnection")
            ?? throw new InvalidOperationException("TobaConnection string is not configured");
    }

    public async Task<List<ExternalClassroomInfo>> GetExternalClassroomsAsync()
    {
        var items = new List<ExternalClassroomInfo>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT nombre, edificacion, piso, capacidad FROM negocio.sga_espacios WHERE estado = 'A' ORDER BY nombre",
            connection);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var nombre = reader.GetString(0);
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                items.Add(new ExternalClassroomInfo
                {
                    Nombre = nombre.Trim(),
                    Edificacion = reader.GetInt32(1),
                    Piso = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Capacidad = reader.IsDBNull(3) ? null : reader.GetInt16(3)
                });
            }
        }

        return items;
    }
}
