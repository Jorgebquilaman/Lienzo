using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class AsistenciaAlumno : BaseEntity
{
    public Guid ClaseId { get; private set; }
    public Clase Clase { get; private set; } = null!;
    public int SgaAlumnoId { get; private set; }
    public int SgaPersonaId { get; private set; }
    public string AlumnoNombre { get; private set; } = string.Empty;
    public string AlumnoDocumento { get; private set; } = string.Empty;
    public bool Presente { get; private set; }
    public Guid? MarcadoPorUsuarioId { get; private set; }
    public DateTime? MarcadoAt { get; private set; }
    public int? SgaAsistenciaId { get; private set; }

    private AsistenciaAlumno() { }

    public AsistenciaAlumno(
        Guid claseId,
        int sgaAlumnoId,
        int sgaPersonaId,
        string alumnoNombre,
        string alumnoDocumento,
        int? sgaAsistenciaId = null)
    {
        Id = Guid.NewGuid();
        ClaseId = claseId;
        SgaAlumnoId = sgaAlumnoId;
        SgaPersonaId = sgaPersonaId;
        AlumnoNombre = alumnoNombre;
        AlumnoDocumento = alumnoDocumento;
        Presente = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarcarPresente(Guid marcadoPorUsuarioId)
    {
        Presente = true;
        MarcadoPorUsuarioId = marcadoPorUsuarioId;
        MarcadoAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarcarAusente()
    {
        Presente = false;
        MarcadoPorUsuarioId = null;
        MarcadoAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void TogglePresencia(Guid marcadoPorUsuarioId)
    {
        if (Presente)
            MarcarAusente();
        else
            MarcarPresente(marcadoPorUsuarioId);
    }
}
