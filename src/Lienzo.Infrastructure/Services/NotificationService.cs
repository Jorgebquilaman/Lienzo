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
    private readonly IRealTimeNotifier _realTimeNotifier;

    public NotificationService(LienzoDbContext context, UserManager<ApplicationUser> userManager, IRealTimeNotifier realTimeNotifier)
    {
        _context = context;
        _userManager = userManager;
        _realTimeNotifier = realTimeNotifier;
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

        await _realTimeNotifier.NotifyUserAsync(userId, new
        {
            id = notification.Id,
            userId = notification.UserId,
            title = notification.Title,
            message = notification.Body,
            type = notification.Type.ToString(),
            isRead = notification.IsRead,
            referenceId = notification.RelatedEntityId,
            createdAt = notification.CreatedAt.ToString("o")
        });

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

        foreach (var user in users)
        {
            var lastNotification = _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .First();

            await _realTimeNotifier.NotifyUserAsync(user.Id, new
            {
                id = lastNotification.Id,
                userId = lastNotification.UserId,
                title = lastNotification.Title,
                message = lastNotification.Body,
                type = lastNotification.Type.ToString(),
                isRead = lastNotification.IsRead,
                referenceId = lastNotification.RelatedEntityId,
                createdAt = lastNotification.CreatedAt.ToString("o")
            });
        }

        return Result<bool>.Success(true);
    }
}
