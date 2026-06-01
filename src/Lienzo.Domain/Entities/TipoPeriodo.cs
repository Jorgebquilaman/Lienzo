using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class TipoPeriodo : BaseEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public int? CodigoExterno { get; private set; }

    private TipoPeriodo() { }

    public TipoPeriodo(string nombre, int? codigoExterno = null)
    {
        Id = Guid.NewGuid();
        Nombre = nombre;
        CodigoExterno = codigoExterno;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCodigoExterno(int codigoExterno)
    {
        CodigoExterno = codigoExterno;
        UpdatedAt = DateTime.UtcNow;
    }
}
