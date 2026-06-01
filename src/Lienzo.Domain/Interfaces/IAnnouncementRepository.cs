using Lienzo.Domain.Entities;

namespace Lienzo.Domain.Interfaces;

public interface IAnnouncementRepository : IRepository<Announcement>
{
    Task<List<Announcement>> GetByTeacherIdAsync(Guid teacherId);
    Task<List<Announcement>> GetByStudentIdAsync(Guid studentId);
    Task MarkAsReadAsync(Guid announcementId, Guid studentId);
}
