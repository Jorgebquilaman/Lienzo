using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace Lienzo.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly LienzoDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(LienzoDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<bool>> SendAsync(
        Guid userId,
        string title,
        string body,
        string type,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        if (!Enum.TryParse<NotificationType>(type, true, out var notificationType))
            notificationType = NotificationType.Info;

        var notification = new Notification(userId, title, body, notificationType, relatedEntityId, relatedEntityType);
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> SendToRoleAsync(
        string role,
        string title,
        string body,
        string type,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);

        if (!Enum.TryParse<NotificationType>(type, true, out var notificationType))
            notificationType = NotificationType.Info;

        foreach (var user in users)
        {
            var notification = new Notification(user.Id, title, body, notificationType, relatedEntityId, relatedEntityType);
            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
}
