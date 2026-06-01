namespace Lienzo.Application.Interfaces;

public class ExternalTipoPeriodoInfo
{
    public int PeriodoGenerico { get; init; }
    public string Nombre { get; init; } = "";
}

public interface ISyncTipoPeriodoService
{
    Task<List<ExternalTipoPeriodoInfo>> GetTiposPeriodoAsync();
}
