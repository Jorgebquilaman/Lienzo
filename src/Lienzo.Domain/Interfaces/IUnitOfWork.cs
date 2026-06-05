using Lienzo.Domain.Entities;

namespace Lienzo.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IBuildingRepository Buildings { get; }
    IClassroomRepository Classrooms { get; }
    IReservationRepository Reservations { get; }
    IAnnouncementRepository Announcements { get; }
    INotificationRepository Notifications { get; }
    IHolidayRepository Holidays { get; }
    IPeriodoRepository Periodos { get; }
    ICarreraRepository Carreras { get; }
    IActividadRepository Actividades { get; }
    ITipoPeriodoRepository TiposPeriodo { get; }
    IRepository<ActividadDocente> ActividadDocentes { get; }
    IRepository<ReservationReminder> ReservationReminders { get; }
    IRepository<MaintenanceBlock> MaintenanceBlocks { get; }
    IRepository<ClassroomSurvey> ClassroomSurveys { get; }
    IRepository<Clase> Clases { get; }
    IRepository<AsistenciaAlumno> AsistenciasAlumnos { get; }
    IRepository<SystemSetting> SystemSettings { get; }
    IRepository<KeyDelivery> KeyDeliveries { get; }
    IRepository<Accessory> Accessories { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
