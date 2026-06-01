namespace Lienzo.Application.Interfaces;

public class ExternalPeriodoInfo
{
    public int Periodo { get; init; }
    public string Nombre { get; init; } = "";
    public short AnioAcademico { get; init; }
    public int PeriodoGenerico { get; init; }
    public DateOnly FechaInicio { get; init; }
    public DateOnly FechaFin { get; init; }
}

public interface ISyncPeriodoService
{
    Task<List<ExternalPeriodoInfo>> GetPeriodosAsync(short anioAcademico);
}
