using Lienzo.Domain.Entities;

namespace Lienzo.Domain.Interfaces;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetUserReservationsAsync(Guid userId);
    Task<IEnumerable<Reservation>> GetPendingAsync();
    Task<bool> HasConflictAsync(
        Guid classroomId,
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeId = null);

    Task<bool> HasConflictForDatesAsync(
        Guid classroomId,
        List<DateOnly> dates,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeId = null);

    Task<IEnumerable<Reservation>> GetByDateRangeAsync(DateOnly fromDate, DateOnly toDate, Guid? buildingId = null, Guid? classroomId = null);

    Task<IEnumerable<Reservation>> GetAllWithDetailsAsync();

    Task DeleteByActividadIdAsync(Guid actividadId);
}
