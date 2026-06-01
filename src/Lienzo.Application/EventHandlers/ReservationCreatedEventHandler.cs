using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Events;
using MediatR;

namespace Lienzo.Application.EventHandlers;

public class ReservationCreatedEventHandler : INotificationHandler<ReservationCreatedEvent>
{
    private readonly INotificationService _notificationService;

    public ReservationCreatedEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(ReservationCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.SendAsync(
            notification.UserId,
            "Reserva creada",
            $"Tu reserva para el {notification.Date:dd/MM/yyyy} de {notification.StartTime:hh\\:mm} a {notification.EndTime:hh\\:mm} está pendiente de aprobación.",
            "Info",
            notification.ReservationId,
            "Reservation");
    }
}
