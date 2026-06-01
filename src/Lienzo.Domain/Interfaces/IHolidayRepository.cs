using Lienzo.Domain.Entities;

namespace Lienzo.Domain.Interfaces;

public interface IHolidayRepository : IRepository<Holiday>
{
    Task<bool> IsHolidayAsync(DateOnly date);
}
