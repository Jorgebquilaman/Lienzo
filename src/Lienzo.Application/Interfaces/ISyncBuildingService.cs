namespace Lienzo.Application.Interfaces;

public class ExternalBuildingInfo
{
    public int Edificacion { get; init; }
    public string Nombre { get; init; } = "";
}

public interface ISyncBuildingService
{
    Task<List<ExternalBuildingInfo>> GetExternalBuildingsAsync();
}
