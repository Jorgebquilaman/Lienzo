namespace Lienzo.Application.Interfaces;

public class SyncEstudianteResult
{
    public int Creados { get; set; }
    public int Existentes { get; set; }
    public int Errores { get; set; }
    public List<string> CorreosErroneos { get; set; } = [];
}

public class ExternalEstudianteInfo
{
    public int AlumnoId { get; init; }
    public int PersonaId { get; init; }
    public string Apellido { get; init; } = "";
    public string Nombres { get; init; } = "";
    public string Usuario { get; init; } = "";
}

public interface ISyncEstudianteService
{
    Task<List<ExternalEstudianteInfo>> GetEstudiantesAsync(short anioAcademico);
    Task<SyncEstudianteResult> CreateMissingEstudianteUsersAsync(short anioAcademico);
}
