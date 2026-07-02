using Lienzo.Domain.Entities;

namespace Lienzo.Domain.Interfaces;

public interface IRecesoRepository : IRepository<Receso>
{
    Task<bool> IsRecesoAsync(DateOnly date);
}
