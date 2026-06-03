namespace Lienzo.Application.Interfaces;

public class ExternalActividadInfo
{
    public int Comision { get; init; }
    public string Nombre { get; init; } = "";
    public string ElementoNombre { get; init; } = "";
    public string ElementoCodigo { get; init; } = "";
    public int PeriodoLectivo { get; init; }
    public int PeriodoId { get; init; }
    public int Edificacion { get; init; }
    public int? PropuestaId { get; init; }
    public string? PropuestaCodigo { get; init; }
    public List<string> DocenteNames { get; set; } = [];

    // Schedule from sga_asignaciones
    public string? DiaSemana { get; init; }
    public TimeOnly? HoraInicio { get; init; }
    public TimeOnly? HoraFin { get; init; }
    public int? EspacioId { get; init; }
    public string? AulaNombre { get; set; }
    public string? DiasDictado { get; init; }
}

public interface ISyncActividadService
{
    Task<List<ExternalActividadInfo>> GetActividadesAsync(short anioAcademico);
}
