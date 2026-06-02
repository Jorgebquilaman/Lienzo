namespace Lienzo.Application.Interfaces;

public class SgaClaseAlumnoInfo
{
    public int SgaClaseId { get; init; }
    public DateOnly Fecha { get; init; }
    public int Comision { get; init; }
    public int AlumnoId { get; init; }
    public int PersonaId { get; init; }
    public string Apellido { get; init; } = "";
    public string Nombres { get; init; } = "";
    public int SgaAsistenciaId { get; init; }
}

public class SyncSgaResult
{
    public int Actualizados { get; set; }
    public int Errores { get; set; }
    public List<string> DetalleErrores { get; set; } = [];
}

public interface ISgaAsistenciaService
{
    Task<List<SgaClaseAlumnoInfo>> GetAlumnosPorComisionFechaAsync(int comision, DateOnly fecha);
    Task<SyncSgaResult> SincronizarAsistenciaAsync(Guid claseId, List<(int SgaAsistenciaId, bool Presente)> asistencias);
}
