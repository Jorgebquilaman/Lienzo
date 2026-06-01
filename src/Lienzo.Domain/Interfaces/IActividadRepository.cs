using Lienzo.Domain.Entities;

namespace Lienzo.Domain.Interfaces;

public interface IActividadRepository : IRepository<Actividad>
{
    Task<IEnumerable<Actividad>> GetAllWithDetailsAsync();
    Task<Actividad?> GetWithDetailsAsync(Guid id);
    Task RemoveAllDocentesAsync(Guid actividadId);
}
