using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Repositories;

public class AnnouncementRepository : GenericRepository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(LienzoDbContext context) : base(context) { }

    public async Task<List<Announcement>> GetByTeacherIdAsync(Guid teacherId)
    {
        return await DbSet
            .Include(a => a.Recipients)
            .Where(a => a.TeacherId == teacherId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Announcement>> GetByStudentIdAsync(Guid studentId)
    {
        return await DbSet
            .Include(a => a.Recipients)
            .Where(a => !a.IsDeleted && (
                a.TargetAudience == TargetAudience.All ||
                a.TargetAudience == TargetAudience.AllStudents ||
                a.Recipients.Any(r => r.StudentId == studentId)))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid announcementId, Guid studentId)
    {
        var announcement = await DbSet
            .Include(a => a.Recipients)
            .FirstOrDefaultAsync(a => a.Id == announcementId);

        if (announcement is not null)
        {
            announcement.MarkAsRead(studentId);
        }
    }
}
