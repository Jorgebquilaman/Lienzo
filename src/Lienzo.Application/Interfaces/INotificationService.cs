using Lienzo.Application.Common.Models;

namespace Lienzo.Application.Interfaces;

public interface INotificationService
{
    Task<Result<bool>> SendAsync(Guid userId, string title, string body, string type, Guid? relatedEntityId = null, string? relatedEntityType = null);
    Task<Result<bool>> SendToRoleAsync(string role, string title, string body, string type, Guid? relatedEntityId = null, string? relatedEntityType = null);
}
