using Lienzo.Domain.Common;
using Lienzo.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Data;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiry { get; set; }
}

public class LienzoDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public LienzoDbContext(DbContextOptions<LienzoDbContext> options) : base(options) { }

    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<AnnouncementRecipient> AnnouncementRecipients => Set<AnnouncementRecipient>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<Periodo> Periodos => Set<Periodo>();
    public DbSet<Carrera> Carreras => Set<Carrera>();
    public DbSet<Actividad> Actividades => Set<Actividad>();
    public DbSet<ActividadDocente> ActividadDocentes => Set<ActividadDocente>();
    public DbSet<TipoPeriodo> TiposPeriodo => Set<TipoPeriodo>();
    public DbSet<ReservationReminder> ReservationReminders => Set<ReservationReminder>();
    public DbSet<MaintenanceBlock> MaintenanceBlocks => Set<MaintenanceBlock>();
    public DbSet<ClassroomSurvey> ClassroomSurveys => Set<ClassroomSurvey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(LienzoDbContext).Assembly);

        builder.Entity<ApplicationUser>(e => e.ToTable("usuarios"));
        builder.Entity<IdentityRole<Guid>>(e => e.ToTable("roles"));
        builder.Entity<IdentityUserRole<Guid>>(e => e.ToTable("usuarios_roles"));
        builder.Entity<IdentityUserClaim<Guid>>(e => e.ToTable("usuarios_claims"));
        builder.Entity<IdentityUserLogin<Guid>>(e => e.ToTable("usuarios_logins"));
        builder.Entity<IdentityUserToken<Guid>>(e => e.ToTable("usuarios_tokens"));
        builder.Entity<IdentityRoleClaim<Guid>>(e => e.ToTable("roles_claims"));

        builder.Entity<Building>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Classroom>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Reservation>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Announcement>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ActividadDocente>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ReservationReminder>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<MaintenanceBlock>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ClassroomSurvey>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
