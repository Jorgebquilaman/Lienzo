using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Repositories;

public class BuildingRepository : GenericRepository<Building>, IBuildingRepository
{
    public BuildingRepository(LienzoDbContext context) : base(context) { }

    public async Task<Building?> GetWithClassroomsAsync(Guid id)
    {
        return await DbSet
            .Include(b => b.Classrooms)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
