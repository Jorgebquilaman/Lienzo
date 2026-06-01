using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lienzo.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LienzoDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
            await roleManager.CreateAsync(new IdentityRole<Guid>("Teacher"));
            await roleManager.CreateAsync(new IdentityRole<Guid>("Student"));
        }

        if (await userManager.FindByEmailAsync("admin@lienzo.edu") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@lienzo.edu",
                Email = "admin@lienzo.edu",
                FirstName = "Admin",
                LastName = "Sistema",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        for (int i = 1; i <= 3; i++)
        {
            var email = $"teacher{i}@lienzo.edu";
            if (await userManager.FindByEmailAsync(email) is null)
            {
                var teacher = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = "Profesor",
                    LastName = $"{i}",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(teacher, "Teacher123!");
                await userManager.AddToRoleAsync(teacher, "Teacher");
            }
        }

        for (int i = 1; i <= 10; i++)
        {
            var email = $"student{i}@lienzo.edu";
            if (await userManager.FindByEmailAsync(email) is null)
            {
                var student = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = "Estudiante",
                    LastName = $"{i}",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(student, "Student123!");
                await userManager.AddToRoleAsync(student, "Student");
            }
        }

        if (await context.Classrooms.AnyAsync())
            return;

        Building? principal = null, artes = null, deportivo = null;

        if (!await context.Buildings.AnyAsync())
        {
            principal = new Building("Edificio Principal", "Av. Central 123", 5);
            artes = new Building("Edificio de Artes", "Calle de la Cultura 456", 3);
            deportivo = new Building("Edificio Deportivo", "Av. del Deporte 789", 2);
            context.Buildings.AddRange(principal, artes, deportivo);
            await context.SaveChangesAsync();
        }
        else
        {
            var buildings = await context.Buildings.ToListAsync();
            principal = buildings.First(b => b.Name.Contains("Principal"));
            artes = buildings.First(b => b.Name.Contains("Artes"));
            deportivo = buildings.First(b => b.Name.Contains("Deportivo"));
        }

        var classrooms = new List<Classroom>
        {
            new("Aula 101", principal!.Id, 1, 30, ClassroomType.General,
                ["Proyector", "Pizarra", "Escritorios", "Aire acondicionado"]),
            new("Aula 102", principal!.Id, 1, 25, ClassroomType.General,
                ["Proyector", "Pizarra", "Escritorios"]),
            new("Aula 201", principal!.Id, 2, 35, ClassroomType.General,
                ["Proyector", "Pizarra", "Escritorios", "Aire acondicionado", "Equipo de sonido"]),
            new("Aula 202", principal!.Id, 2, 20, ClassroomType.General,
                ["Proyector", "Pizarra", "Escritorios"]),
            new("Aula 301", principal!.Id, 3, 40, ClassroomType.General,
                ["Proyector", "Pizarra", "Escritorios", "Aire acondicionado"]),
            new("Sala de Danza 1", artes!.Id, 1, 20, ClassroomType.Dance,
                ["Espejos", "Barras de ballet", "Equipo de sonido", "Piso de madera"]),
            new("Sala de Danza 2", artes!.Id, 1, 15, ClassroomType.Dance,
                ["Espejos", "Barras de ballet", "Equipo de sonido"]),
            new("Taller de Dibujo 1", artes!.Id, 2, 20, ClassroomType.Drawing,
                ["Caballetes", "Bodegones", "Iluminación natural", "Lavamanos"]),
            new("Taller de Dibujo 2", artes!.Id, 2, 15, ClassroomType.Drawing,
                ["Caballetes", "Bodegones", "Iluminación natural"]),
            new("Taller de Dibujo 3", artes!.Id, 2, 25, ClassroomType.Drawing,
                ["Caballetes", "Bodegones", "Iluminación natural", "Lavamanos", "Estanterías"]),
            new("Sala de Música 1", artes!.Id, 3, 15, ClassroomType.Music,
                ["Piano", "Atriles", "Paneles acústicos", "Metrónomo"]),
            new("Sala de Música 2", artes!.Id, 3, 10, ClassroomType.Music,
                ["Piano", "Atriles", "Paneles acústicos"]),
            new("Cancha Cubierta", deportivo!.Id, 1, 50, ClassroomType.General,
                ["Proyector", "Equipo de sonido", "Aire acondicionado", "Vestidores"]),
            new("Salón Multiusos", deportivo!.Id, 1, 30, ClassroomType.General,
                ["Proyector", "Pizarra", "Escritorios"]),
            new("Sala de Estiramientos", deportivo!.Id, 2, 20, ClassroomType.Dance,
                ["Espejos", "Barras de ballet", "Equipo de sonido", "Colchonetas"])
        };

        context.Classrooms.AddRange(classrooms);
        await context.SaveChangesAsync();
    }
}
