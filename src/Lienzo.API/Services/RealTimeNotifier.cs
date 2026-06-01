using Lienzo.API.Hubs;
using Lienzo.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Lienzo.API.Services;

public class RealTimeNotifier : IRealTimeNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public RealTimeNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserAsync(Guid userId, object payload, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", payload, ct);
    }

    public async Task NotifyRoleAsync(string role, object payload, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(role).SendAsync("ReceiveNotification", payload, ct);
    }
}
