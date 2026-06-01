using Lienzo.Domain.Enums;

namespace Lienzo.Application.Interfaces;

public class ExternalClassroomInfo
{
    public string Nombre { get; init; } = "";
    public int Edificacion { get; init; }
    public string? Piso { get; init; }
    public short? Capacidad { get; init; }
}

public interface ISyncClassroomService
{
    Task<List<ExternalClassroomInfo>> GetExternalClassroomsAsync();
}
