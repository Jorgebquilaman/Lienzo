namespace Lienzo.Application.Interfaces;

public class ExternalCarreraInfo
{
    public int Propuesta { get; init; }
    public string Nombre { get; init; } = "";
    public string Codigo { get; init; } = "";
}

public interface ISyncCarreraService
{
    Task<List<ExternalCarreraInfo>> GetCarrerasAsync();
}
