using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;

namespace Lienzo.Infrastructure.Repositories;

public class TipoPeriodoRepository : GenericRepository<TipoPeriodo>, ITipoPeriodoRepository
{
    public TipoPeriodoRepository(LienzoDbContext context) : base(context) { }
}
