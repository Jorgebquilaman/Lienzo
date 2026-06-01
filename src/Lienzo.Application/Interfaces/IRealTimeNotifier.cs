namespace Lienzo.Application.Interfaces;

public interface IRealTimeNotifier
{
    Task NotifyUserAsync(Guid userId, object payload, CancellationToken ct = default);
    Task NotifyRoleAsync(string role, object payload, CancellationToken ct = default);
}
