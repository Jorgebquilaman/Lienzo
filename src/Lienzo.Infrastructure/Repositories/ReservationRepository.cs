using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Repositories;

public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(LienzoDbContext context) : base(context) { }

    public async Task<IEnumerable<Reservation>> GetUserReservationsAsync(Guid userId)
    {
        return await DbSet
            .Include(r => r.Classroom)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetPendingAsync()
    {
        return await DbSet
            .Include(r => r.Classroom)
            .Include(r => r.Classroom.Building)
            .Where(r => r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<bool> HasConflictAsync(
        Guid classroomId,
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeId = null)
    {
        var dateOnly = DateOnly.FromDateTime(date);

        var query = DbSet.Where(r =>
            r.ClassroomId == classroomId &&
            r.Date == dateOnly &&
            r.Status != ReservationStatus.Cancelled &&
            r.Status != ReservationStatus.Rejected &&
            r.StartTime < endTime &&
            r.EndTime > startTime);

        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> HasConflictForDatesAsync(
        Guid classroomId,
        List<DateOnly> dates,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeId = null)
    {
        if (dates.Count == 0) return false;

        var query = DbSet.Where(r =>
            r.ClassroomId == classroomId &&
            dates.Contains(r.Date) &&
            r.Status != ReservationStatus.Cancelled &&
            r.Status != ReservationStatus.Rejected &&
            r.StartTime < endTime &&
            r.EndTime > startTime);

        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByDateRangeAsync(
        DateOnly fromDate, DateOnly toDate, Guid? buildingId = null, Guid? classroomId = null)
    {
        var query = DbSet
            .Include(r => r.Classroom)
            .Where(r => !r.IsDeleted && r.Date >= fromDate && r.Date <= toDate
                && r.Status != ReservationStatus.Rejected && r.Status != ReservationStatus.Cancelled);

        if (classroomId.HasValue)
            query = query.Where(r => r.ClassroomId == classroomId.Value);
        else if (buildingId.HasValue)
            query = query.Where(r => r.Classroom.BuildingId == buildingId.Value);

        return await query
            .OrderBy(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetAllWithDetailsAsync()
    {
        return await DbSet
            .Include(r => r.Classroom)
                .ThenInclude(c => c.Building)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task DeleteByActividadIdAsync(Guid actividadId)
    {
        var reservations = await DbSet.Where(r => r.ActividadId == actividadId && !r.IsDeleted).ToListAsync();
        foreach (var r in reservations)
            Delete(r);
    }
}
