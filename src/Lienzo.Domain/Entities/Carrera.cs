using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class Carrera : BaseEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public int? CodigoExterno { get; private set; }

    private Carrera() { }

    public Carrera(string nombre, string codigo, int? codigoExterno = null)
    {
        Id = Guid.NewGuid();
        Nombre = nombre;
        Codigo = codigo;
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
