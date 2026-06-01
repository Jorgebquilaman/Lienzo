using Lienzo.Domain.Common;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Repositories;
using MediatR;

namespace Lienzo.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly LienzoDbContext _context;
    private readonly IMediator _mediator;

    public IBuildingRepository Buildings { get; }
    public IClassroomRepository Classrooms { get; }
    public IReservationRepository Reservations { get; }
    public IAnnouncementRepository Announcements { get; }
    public INotificationRepository Notifications { get; }
    public IHolidayRepository Holidays { get; }
    public IPeriodoRepository Periodos { get; }
    public ICarreraRepository Carreras { get; }
    public IActividadRepository Actividades { get; }
    public ITipoPeriodoRepository TiposPeriodo { get; }
    public IRepository<ActividadDocente> ActividadDocentes { get; }
    public IRepository<ReservationReminder> ReservationReminders { get; }
    public IRepository<MaintenanceBlock> MaintenanceBlocks { get; }
    public IRepository<ClassroomSurvey> ClassroomSurveys { get; }

    public UnitOfWork(
        LienzoDbContext context,
        IMediator mediator,
        IBuildingRepository buildings,
        IClassroomRepository classrooms,
        IReservationRepository reservations,
        IAnnouncementRepository announcements,
        INotificationRepository notifications,
        IHolidayRepository holidays,
        IPeriodoRepository periodos,
        ICarreraRepository carreras,
        IActividadRepository actividades,
        ITipoPeriodoRepository tiposPeriodo)
    {
        _context = context;
        _mediator = mediator;
        Buildings = buildings;
        Classrooms = classrooms;
        Reservations = reservations;
        Announcements = announcements;
        Notifications = notifications;
        Holidays = holidays;
        Periodos = periodos;
        Carreras = carreras;
        Actividades = actividades;
        TiposPeriodo = tiposPeriodo;
        ActividadDocentes = new GenericRepository<ActividadDocente>(context);
        ReservationReminders = new GenericRepository<ReservationReminder>(context);
        MaintenanceBlocks = new GenericRepository<MaintenanceBlock>(context);
        ClassroomSurveys = new GenericRepository<ClassroomSurvey>(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        domainEntities.ForEach(e => e.Entity.ClearDomainEvents());

        var result = await _context.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
            await _mediator.Publish(domainEvent, cancellationToken);

        return result;
    }

    public void Dispose() => _context.Dispose();
}
