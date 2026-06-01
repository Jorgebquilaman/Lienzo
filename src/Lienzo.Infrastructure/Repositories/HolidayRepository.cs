using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Repositories;

public class HolidayRepository : GenericRepository<Holiday>, IHolidayRepository
{
    public HolidayRepository(LienzoDbContext context) : base(context) { }

    public override async Task<IEnumerable<Holiday>> GetAllAsync()
    {
        return await DbSet.Where(h => !h.IsDeleted).ToListAsync();
    }

    public async Task<bool> IsHolidayAsync(DateOnly date)
    {
        return await DbSet.AnyAsync(h => h.Date == date && !h.IsDeleted);
    }
}
