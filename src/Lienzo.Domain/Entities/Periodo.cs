using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class Periodo : BaseEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public int Anio { get; private set; }
    public int? CodigoExterno { get; private set; }
    public Guid? TipoPeriodoId { get; private set; }
    public TipoPeriodo? TipoPeriodo { get; private set; }

    private Periodo() { }

    public Periodo(string nombre, DateOnly fechaInicio, DateOnly fechaFin, int anio, int? codigoExterno = null, Guid? tipoPeriodoId = null)
    {
        Id = Guid.NewGuid();
        Nombre = nombre;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Anio = anio;
        CodigoExterno = codigoExterno;
        TipoPeriodoId = tipoPeriodoId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCodigoExterno(int codigoExterno)
    {
        CodigoExterno = codigoExterno;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTipoPeriodo(Guid tipoPeriodoId)
    {
        TipoPeriodoId = tipoPeriodoId;
        UpdatedAt = DateTime.UtcNow;
    }
}
