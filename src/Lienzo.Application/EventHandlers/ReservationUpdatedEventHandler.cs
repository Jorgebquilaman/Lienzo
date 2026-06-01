using Lienzo.Application.Interfaces;
using Lienzo.Domain.Events;
using MediatR;

namespace Lienzo.Application.EventHandlers;

public class ReservationUpdatedEventHandler : INotificationHandler<ReservationUpdatedEvent>
{
    private readonly INotificationService _notificationService;

    public ReservationUpdatedEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(ReservationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.SendAsync(
            notification.UserId,
            "Reserva actualizada",
            "Los detalles de tu reserva han sido modificados.",
            "Info",
            notification.ReservationId,
            "Reservation");
    }
}
