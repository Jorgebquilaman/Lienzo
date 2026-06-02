using Lienzo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

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
            @"SELECT c.comision, c.nombre, c.nombre || ' - ' || e.nombre, e.codigo, pl.periodo_lectivo, p.periodo, COALESCE(esp.edificacion, 0), cp.propuesta, sp.codigo
              FROM negocio.sga_comisiones c
              JOIN negocio.sga_periodos_lectivos pl ON pl.periodo_lectivo = c.periodo_lectivo
              JOIN negocio.sga_periodos p ON p.periodo = pl.periodo
              JOIN negocio.sga_elementos e ON e.elemento = c.elemento
              LEFT JOIN negocio.sga_espacios esp ON esp.espacio = c.ubicacion
              LEFT JOIN negocio.sga_comisiones_propuestas cp ON cp.comision = c.comision
              LEFT JOIN negocio.sga_propuestas sp ON sp.propuesta = cp.propuesta
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
                PropuestaCodigo = r.IsDBNull(8) ? null : r.GetString(8).Trim(),
            });
        }

        await r.CloseAsync();

        // --- Load schedule data (sga_comisiones_bh -> sga_asignaciones) ---
        var comisionIds = items.Select(i => i.Comision).ToList();
        if (comisionIds.Count > 0)
        {
            var scheduleMap = new Dictionary<int, (string DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin, int? EspacioId)>();

            await using var hCmd = new NpgsqlCommand(
                @"SELECT bh.comision, a.dia_semana, a.hora_inicio, a.hora_finalizacion, a.espacio
                  FROM negocio.sga_comisiones_bh bh
                  JOIN negocio.sga_asignaciones a ON a.asignacion = bh.asignacion
                  JOIN negocio.sga_comisiones c ON c.comision = bh.comision AND c.estado = 'A'
                  JOIN negocio.sga_periodos_lectivos pl ON pl.periodo_lectivo = c.periodo_lectivo
                  JOIN negocio.sga_periodos p ON p.periodo = pl.periodo AND p.anio_academico = @anio2
                  ORDER BY bh.comision, a.dia_semana",
                conn);
            hCmd.Parameters.AddWithValue("anio2", anioAcademico);

            await using var hr = await hCmd.ExecuteReaderAsync();
            while (await hr.ReadAsync())
            {
                var comisionId = hr.GetInt32(0);
                var dia = hr.IsDBNull(1) ? "" : hr.GetString(1).Trim();
                var hi = hr.IsDBNull(2) ? (TimeOnly?)null : hr.GetFieldValue<TimeOnly>(2);
                var hf = hr.IsDBNull(3) ? (TimeOnly?)null : hr.GetFieldValue<TimeOnly>(3);
                var espacio = hr.IsDBNull(4) ? (int?)null : hr.GetInt32(4);

                if (string.IsNullOrEmpty(dia) || hi is null || hf is null)
                    continue;

                // Store only the first schedule entry per comision
                if (!scheduleMap.ContainsKey(comisionId))
                {
                    scheduleMap[comisionId] = (dia, hi.Value, hf.Value, espacio);
                }
            }
            await hr.CloseAsync();

            Console.WriteLine($"[SyncActividadService] scheduleMap has {scheduleMap.Count} entries");

            // Resolve espacio IDs to aula names
            var espacioIds = scheduleMap.Values
                .Where(v => v.EspacioId.HasValue)
                .Select(v => v.EspacioId!.Value)
                .Distinct()
                .ToList();

            var espacioMap = new Dictionary<int, string>();
            if (espacioIds.Count > 0)
            {
                await using var eCmd = new NpgsqlCommand(
                    @"SELECT espacio, nombre FROM negocio.sga_espacios WHERE espacio = ANY(@ids)",
                    conn);
                eCmd.Parameters.AddWithValue("ids", espacioIds);

                await using var er = await eCmd.ExecuteReaderAsync();
                while (await er.ReadAsync())
                {
                    var id = er.GetInt32(0);
                    var name = er.IsDBNull(1) ? "" : er.GetString(1).Trim();
                    if (!string.IsNullOrEmpty(name))
                        espacioMap[id] = name;
                }
                await er.CloseAsync();
            }

            // Apply schedule to items
            foreach (var item in items)
            {
                if (scheduleMap.TryGetValue(item.Comision, out var sched))
                {
                    // Use reflection-free approach: just set the init-only props
                    // We'll handle this differently - use a separate dictionary approach
                }
            }

            // Apply schedule to items - use a second loop with constructor-like approach
            var updatedItems = new List<ExternalActividadInfo>();
            foreach (var item in items)
            {
                if (scheduleMap.TryGetValue(item.Comision, out var sched))
                {
                    var aulaNombre = sched.EspacioId.HasValue
                        ? espacioMap.GetValueOrDefault(sched.EspacioId.Value)
                        : null;

                    updatedItems.Add(new ExternalActividadInfo
                    {
                        Comision = item.Comision,
                        Nombre = item.Nombre,
                        ElementoNombre = item.ElementoNombre,
                        ElementoCodigo = item.ElementoCodigo,
                        PeriodoLectivo = item.PeriodoLectivo,
                        PeriodoId = item.PeriodoId,
                        Edificacion = item.Edificacion,
                        PropuestaId = item.PropuestaId,
                        PropuestaCodigo = item.PropuestaCodigo,
                        DocenteNames = item.DocenteNames,
                        DiaSemana = sched.DiaSemana,
                        HoraInicio = sched.HoraInicio,
                        HoraFin = sched.HoraFin,
                        EspacioId = sched.EspacioId,
                        AulaNombre = aulaNombre,
                    });
                }
                else
                {
                    updatedItems.Add(item);
                }
            }
            items = updatedItems;
        }

        // --- Load docentes for each comision ---
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
            while (await dr.ReadAsync())
            {
                var comisionId = dr.GetInt32(0);
                var apellido = dr.IsDBNull(1) ? "" : dr.GetString(1).Trim();
                var nombres = dr.IsDBNull(2) ? "" : dr.GetString(2).Trim();
                var fullName = $"{nombres} {apellido}".Trim();
                if (!string.IsNullOrWhiteSpace(fullName) && docenteMap.ContainsKey(comisionId))
                    docenteMap[comisionId].Add(fullName);
            }
            await dr.CloseAsync();

            // Rebuild items with docentes
            var itemsWithDocentes = new List<ExternalActividadInfo>();
            foreach (var item in items)
            {
                itemsWithDocentes.Add(new ExternalActividadInfo
                {
                    Comision = item.Comision,
                    Nombre = item.Nombre,
                    ElementoNombre = item.ElementoNombre,
                    ElementoCodigo = item.ElementoCodigo,
                    PeriodoLectivo = item.PeriodoLectivo,
                    PeriodoId = item.PeriodoId,
                    Edificacion = item.Edificacion,
                    PropuestaId = item.PropuestaId,
                    PropuestaCodigo = item.PropuestaCodigo,
                    DocenteNames = docenteMap.GetValueOrDefault(item.Comision) ?? [],
                    DiaSemana = item.DiaSemana,
                    HoraInicio = item.HoraInicio,
                    HoraFin = item.HoraFin,
                    EspacioId = item.EspacioId,
                    AulaNombre = item.AulaNombre,
                });
            }
            items = itemsWithDocentes;
        }

        return items;
    }
}
