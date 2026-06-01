using Lienzo.Domain.Entities;

namespace Lienzo.Domain.Interfaces;

public interface IBuildingRepository : IRepository<Building>
{
    Task<Building?> GetWithClassroomsAsync(Guid id);
}
