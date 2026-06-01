using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class ActividadDocente : BaseEntity
{
    public Guid ActividadId { get; private set; }
    public string DocenteId { get; private set; } = string.Empty;

    public Actividad Actividad { get; private set; } = null!;

    private ActividadDocente() { }

    public ActividadDocente(Guid actividadId, string docenteId)
    {
        Id = Guid.NewGuid();
        ActividadId = actividadId;
        DocenteId = docenteId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
