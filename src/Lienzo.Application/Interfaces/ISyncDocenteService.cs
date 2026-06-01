namespace Lienzo.Application.Interfaces;

public interface ISyncDocenteService
{
    Task<SyncDocenteResult> CreateMissingDocenteUsersAsync(short anioAcademico);
}

public class SyncDocenteResult
{
    public int Creados { get; set; }
    public int Existentes { get; set; }
    public int Errores { get; set; }
    public List<string> NombresErroneos { get; set; } = [];
}
