using Lienzo.Application.Interfaces;
using Lienzo.Domain.Events;
using MediatR;

namespace Lienzo.Application.EventHandlers;

public class ReservationStatusChangedEventHandler : INotificationHandler<ReservationStatusChangedEvent>
{
    private readonly INotificationService _notificationService;

    public ReservationStatusChangedEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(ReservationStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        var type = notification.NewStatus switch
        {
            "Approved" => "Success",
            "Rejected" => "Error",
            "Cancelled" => "Warning",
            _ => "Info"
        };

        await _notificationService.SendAsync(
            notification.UserId,
            "Estado de reserva actualizado",
            $"Tu reserva ha sido {notification.NewStatus.ToLower()}.",
            type,
            notification.ReservationId,
            "Reservation");
    }
}
