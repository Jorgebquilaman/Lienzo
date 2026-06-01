using Lienzo.Domain.Common;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly LienzoDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(LienzoDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var entry = await DbSet.AddAsync(entity);
        return entry.Entity;
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        DbSet.Update(entity);
    }

    public virtual IQueryable<T> Query()
    {
        return DbSet.AsQueryable();
    }
}
