using Lienzo.Domain.Common;
using Lienzo.Domain.Enums;

namespace Lienzo.Domain.Entities;

public class Clase : BaseEntity
{
    public Guid ReservationId { get; private set; }
    public Reservation Reservation { get; private set; } = null!;
    public Guid ActividadId { get; private set; }
    public Actividad Actividad { get; private set; } = null!;
    public Guid ClassroomId { get; private set; }
    public Classroom Classroom { get; private set; } = null!;
    public DateOnly Fecha { get; private set; }
    public TimeOnly HoraInicio { get; private set; }
    public TimeOnly HoraFin { get; private set; }
    public int SgaComisionId { get; private set; }
    public int? SgaClaseId { get; private set; }
    public DateTime CheckedInAt { get; private set; }
    public Guid CheckedInByUserId { get; private set; }
    public ClaseEstado Estado { get; private set; }

    private readonly List<AsistenciaAlumno> _asistencias = [];
    public IReadOnlyCollection<AsistenciaAlumno> Asistencias => _asistencias.AsReadOnly();

    private Clase() { }

    public Clase(
        Guid reservationId,
        Guid actividadId,
        Guid classroomId,
        DateOnly fecha,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        int sgaComisionId,
        int? sgaClaseId,
        Guid checkedInByUserId)
    {
        Id = Guid.NewGuid();
        ReservationId = reservationId;
        ActividadId = actividadId;
        ClassroomId = classroomId;
        Fecha = fecha;
        HoraInicio = horaInicio;
        HoraFin = horaFin;
        SgaComisionId = sgaComisionId;
        SgaClaseId = sgaClaseId;
        CheckedInAt = DateTime.UtcNow;
        CheckedInByUserId = checkedInByUserId;
        Estado = ClaseEstado.Abierta;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AgregarAsistencia(AsistenciaAlumno asistencia)
    {
        _asistencias.Add(asistencia);
    }

    public void AgregarAsistencias(IEnumerable<AsistenciaAlumno> asistencias)
    {
        _asistencias.AddRange(asistencias);
    }

    public void Cerrar()
    {
        Estado = ClaseEstado.Cerrada;
        UpdatedAt = DateTime.UtcNow;
    }
}
