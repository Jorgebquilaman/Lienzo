using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Repositories;

public class RecesoRepository : GenericRepository<Receso>, IRecesoRepository
{
    public RecesoRepository(LienzoDbContext context) : base(context) { }

    public override async Task<IEnumerable<Receso>> GetAllAsync()
    {
        return await DbSet.Where(r => !r.IsDeleted).OrderBy(r => r.StartDate).ToListAsync();
    }

    public async Task<bool> IsRecesoAsync(DateOnly date)
    {
        return await DbSet.AnyAsync(r => r.StartDate <= date && r.EndDate >= date && !r.IsDeleted);
    }
}
