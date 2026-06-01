using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class Actividad : BaseEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public string CodigoMateria { get; private set; } = string.Empty;
    public Guid PeriodoId { get; private set; }
    public Guid CarreraId { get; private set; }

    // Optional schedule (comision)
    public Guid? AulaId { get; set; }
    public string? DiaSemana { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }

    // Navigation
    public Periodo Periodo { get; private set; } = null!;
    public Carrera Carrera { get; private set; } = null!;
    public Classroom? Aula { get; set; }

    public int? CodigoExterno { get; private set; }

    private readonly List<ActividadDocente> _docentes = [];
    public IReadOnlyCollection<ActividadDocente> Docentes => _docentes.AsReadOnly();

    private Actividad() { }

    public Actividad(string nombre, string codigoMateria, Guid periodoId, Guid carreraId, int? codigoExterno = null)
    {
        Id = Guid.NewGuid();
        Nombre = nombre;
        CodigoMateria = codigoMateria;
        PeriodoId = periodoId;
        CarreraId = carreraId;
        CodigoExterno = codigoExterno;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignSchedule(Guid aulaId, string diaSemana, TimeOnly horaInicio, TimeOnly horaFin)
    {
        AulaId = aulaId;
        DiaSemana = diaSemana;
        HoraInicio = horaInicio;
        HoraFin = horaFin;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveSchedule()
    {
        AulaId = null;
        DiaSemana = null;
        HoraInicio = null;
        HoraFin = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(string nombre, string codigoMateria, Guid periodoId, Guid carreraId)
    {
        Nombre = nombre;
        CodigoMateria = codigoMateria;
        PeriodoId = periodoId;
        CarreraId = carreraId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddDocente(string userId)
    {
        if (!_docentes.Any(d => d.DocenteId == userId))
            _docentes.Add(new ActividadDocente(Id, userId));
    }

    public void RemoveDocente(string userId)
    {
        var docente = _docentes.FirstOrDefault(d => d.DocenteId == userId);
        if (docente is not null)
            _docentes.Remove(docente);
    }

    public void ClearDocentes()
    {
        _docentes.Clear();
    }
}
