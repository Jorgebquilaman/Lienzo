using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Repositories;

public class ClassroomRepository : GenericRepository<Classroom>, IClassroomRepository
{
    private readonly IHolidayRepository _holidayRepository;

    public ClassroomRepository(LienzoDbContext context, IHolidayRepository holidayRepository) : base(context)
    {
        _holidayRepository = holidayRepository;
    }

    public override async Task<IEnumerable<Classroom>> GetAllAsync()
    {
        return await DbSet.Include(c => c.Building).ToListAsync();
    }

    public async Task<IEnumerable<Classroom>> GetAvailableAsync(
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime,
        ClassroomType? type,
        int? minCapacity,
        Guid? buildingId)
    {
        var query = DbSet
            .Include(c => c.Reservations)
            .Where(c => c.IsActive)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        if (minCapacity.HasValue)
            query = query.Where(c => c.Capacity >= minCapacity.Value);

        if (buildingId.HasValue)
            query = query.Where(c => c.BuildingId == buildingId.Value);

        var dateOnly = DateOnly.FromDateTime(date);

        if (dateOnly.DayOfWeek == DayOfWeek.Sunday)
            return [];

        if (dateOnly.DayOfWeek == DayOfWeek.Saturday && (startTime >= new TimeOnly(16, 0) || endTime > new TimeOnly(16, 0)))
            return [];

        if (await _holidayRepository.IsHolidayAsync(dateOnly))
            return [];

        var classrooms = await query.ToListAsync();

        return classrooms.Where(c =>
            !c.Reservations.Any(r =>
                r.Date == dateOnly &&
                r.Status != ReservationStatus.Cancelled &&
                r.Status != ReservationStatus.Rejected &&
                r.StartTime < endTime &&
                r.EndTime > startTime));
    }

    public async Task<Classroom?> GetWithReservationsAsync(Guid id)
    {
        return await DbSet
            .Include(c => c.Reservations)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
