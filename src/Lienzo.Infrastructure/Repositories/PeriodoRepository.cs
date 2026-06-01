using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;

namespace Lienzo.Infrastructure.Repositories;

public class PeriodoRepository : GenericRepository<Periodo>, IPeriodoRepository
{
    public PeriodoRepository(LienzoDbContext context) : base(context) { }
}
