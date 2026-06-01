using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lienzo.Infrastructure;

public class LienzoDbContextFactory : IDesignTimeDbContextFactory<LienzoDbContext>
{
    public LienzoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LienzoDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=lienzo;Username=postgres;Password=postgres");
        return new LienzoDbContext(optionsBuilder.Options);
    }
}
