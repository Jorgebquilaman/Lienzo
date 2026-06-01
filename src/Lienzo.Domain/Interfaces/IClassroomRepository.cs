using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;

namespace Lienzo.Domain.Interfaces;

public interface IClassroomRepository : IRepository<Classroom>
{
    Task<IEnumerable<Classroom>> GetAvailableAsync(
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime,
        ClassroomType? type,
        int? minCapacity,
        Guid? buildingId);

    Task<Classroom?> GetWithReservationsAsync(Guid id);
}
