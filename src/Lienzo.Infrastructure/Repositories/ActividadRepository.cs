using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Repositories;

public class ActividadRepository : GenericRepository<Actividad>, IActividadRepository
{
    public ActividadRepository(LienzoDbContext context) : base(context) { }

    public async Task<IEnumerable<Actividad>> GetAllWithDetailsAsync()
    {
        return await DbSet
            .Include(a => a.Periodo)
            .Include(a => a.Carrera)
            .Include(a => a.Aula)
            .Include(a => a.Docentes.Where(d => !d.IsDeleted))
            .Where(a => !a.IsDeleted)
            .ToListAsync();
    }

    public async Task<Actividad?> GetWithDetailsAsync(Guid id)
    {
        return await DbSet
            .Include(a => a.Periodo)
            .Include(a => a.Carrera)
            .Include(a => a.Aula)
            .Include(a => a.Docentes.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task RemoveAllDocentesAsync(Guid actividadId)
    {
        var docentes = await Context.Set<ActividadDocente>()
            .Where(d => d.ActividadId == actividadId)
            .ToListAsync();
        Context.Set<ActividadDocente>().RemoveRange(docentes);
    }
}
