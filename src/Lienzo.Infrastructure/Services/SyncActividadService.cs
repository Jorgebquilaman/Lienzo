using Lienzo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Lienzo.Infrastructure.Services;

public class SyncActividadService : ISyncActividadService
{
    private readonly string _connectionString;

    public SyncActividadService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TobaConnection")
            ?? throw new InvalidOperationException("TobaConnection string is not configured");
    }

    public async Task<List<ExternalActividadInfo>> GetActividadesAsync(short anioAcademico)
    {
        var items = new List<ExternalActividadInfo>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT c.comision, c.nombre, e.nombre, e.codigo, pl.periodo_lectivo, p.periodo, COALESCE(esp.edificacion, 0), cp.propuesta
              FROM negocio.sga_comisiones c
              JOIN negocio.sga_periodos_lectivos pl ON pl.periodo_lectivo = c.periodo_lectivo
              JOIN negocio.sga_periodos p ON p.periodo = pl.periodo
              JOIN negocio.sga_elementos e ON e.elemento = c.elemento
              LEFT JOIN negocio.sga_espacios esp ON esp.espacio = c.ubicacion
              LEFT JOIN negocio.sga_comisiones_propuestas cp ON cp.comision = c.comision
              WHERE c.estado = 'A'
                AND p.anio_academico = @anio
              ORDER BY e.nombre, c.nombre",
            conn);
        cmd.Parameters.AddWithValue("anio", anioAcademico);

        await using var r = await cmd.ExecuteReaderAsync();

        while (await r.ReadAsync())
        {
            var comisionNombre = r.IsDBNull(1) ? "" : r.GetString(1).Trim();
            var elementoNombre = r.GetString(2).Trim();

            items.Add(new ExternalActividadInfo
            {
                Comision = r.GetInt32(0),
                Nombre = comisionNombre,
                ElementoNombre = elementoNombre,
                ElementoCodigo = r.IsDBNull(3) ? "" : r.GetString(3).Trim(),
                PeriodoLectivo = r.GetInt32(4),
                PeriodoId = r.GetInt32(5),
                Edificacion = r.GetInt32(6),
                PropuestaId = r.IsDBNull(7) ? null : r.GetInt32(7),
            });
        }

        await r.CloseAsync();

        // Load docentes for each comision — fetch ALL for 2026 and match in C#
        var comisionIds = items.Select(i => i.Comision).ToList();
        if (comisionIds.Count > 0)
            {
                var comisionSet = new HashSet<int>(comisionIds);
                var docenteMap = new Dictionary<int, List<string>>();
                foreach (var cid in comisionIds)
                    docenteMap[cid] = [];

                await using var dCmd = new NpgsqlCommand(
                    @"SELECT dc.comision, p.apellido, p.nombres
                      FROM negocio.sga_docentes_comision dc
                      JOIN negocio.sga_docentes d ON d.docente = dc.docente
                      JOIN negocio.mdp_personas p ON p.persona = d.persona
                      JOIN negocio.sga_comisiones c ON c.comision = dc.comision AND c.estado = 'A'
                      JOIN negocio.sga_periodos_lectivos pl ON pl.periodo_lectivo = c.periodo_lectivo
                      JOIN negocio.sga_periodos pe ON pe.periodo = pl.periodo AND pe.anio_academico = @anio
                      WHERE (dc.fecha_desde IS NULL OR dc.fecha_desde <= CURRENT_DATE)
                        AND (dc.fecha_hasta IS NULL OR dc.fecha_hasta >= CURRENT_DATE)
                      ORDER BY dc.comision",
                    conn);
                dCmd.Parameters.AddWithValue("anio", anioAcademico);

                await using var dr = await dCmd.ExecuteReaderAsync();
                var docenteCount = 0;
                while (await dr.ReadAsync())
                {
                    docenteCount++;
                    var comisionId = dr.GetInt32(0);
                    var apellido = dr.IsDBNull(1) ? "" : dr.GetString(1).Trim();
                    var nombres = dr.IsDBNull(2) ? "" : dr.GetString(2).Trim();
                    var fullName = $"{nombres} {apellido}".Trim();
                    if (!string.IsNullOrWhiteSpace(fullName) && docenteMap.ContainsKey(comisionId))
                        docenteMap[comisionId].Add(fullName);
                }
                await dr.CloseAsync();

                foreach (var item in items)
                    item.DocenteNames = docenteMap.GetValueOrDefault(item.Comision) ?? [];
            }

        return items;
    }
}
