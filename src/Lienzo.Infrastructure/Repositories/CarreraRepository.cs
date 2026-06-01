using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;

namespace Lienzo.Infrastructure.Repositories;

public class CarreraRepository : GenericRepository<Carrera>, ICarreraRepository
{
    public CarreraRepository(LienzoDbContext context) : base(context) { }
}
