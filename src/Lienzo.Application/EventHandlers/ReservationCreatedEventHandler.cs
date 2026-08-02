using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Events;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lienzo.Application.EventHandlers;

public class ReservationCreatedEventHandler : INotificationHandler<ReservationCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ISystemSettingService _settings;
    private readonly IAuthService _authService;
    private readonly ILogger<ReservationCreatedEventHandler> _logger;

    public ReservationCreatedEventHandler(
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ISystemSettingService settings,
        IAuthService authService,
        ILogger<ReservationCreatedEventHandler> logger)
    {
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _settings = settings;
        _authService = authService;
        _logger = logger;
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

        try
        {
            await SendAccessoryConfirmationIfNeededAsync(notification.ReservationId, notification.ClassroomId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send accessory confirmation email for reservation {ReservationId}", notification.ReservationId);
        }
    }

    private async Task SendAccessoryConfirmationIfNeededAsync(Guid reservationId, Guid classroomId, CancellationToken ct)
    {
        var reservation = await _unitOfWork.Reservations.Query()
            .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted, ct);
        if (reservation is null)
            return;

        var classroom = await _unitOfWork.Classrooms.GetWithReservationsAsync(classroomId);
        if (classroom is null)
            return;

        var catalogAccessories = classroom.ClassroomAccessories
            .Where(ca => ca.Accessory is { IsActive: true })
            .Select(ca => (ca.Accessory.Name, AccessoryOrigin.Catalog))
            .ToList();

        var featureAccessories = classroom.Features
            .Select(f => (f, AccessoryOrigin.Feature))
            .ToList();

        var all = catalogAccessories.Concat(featureAccessories).ToList();
        if (all.Count == 0)
            return;

        var token = Guid.NewGuid().ToString("N");
        reservation.RequireAccessoryConfirmation(token, all);
        await _unitOfWork.SaveChangesAsync(ct);

        var usersResult = await _authService.GetAllUsersAsync();
        var userEmail = usersResult.IsSuccess
            ? usersResult.Value.FirstOrDefault(u => u.Id == reservation.UserId)?.Email
            : null;
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("Cannot send accessory confirmation email: user {UserId} has no email", reservation.UserId);
            return;
        }

        var publicUrl = await _settings.GetValueAsync("PublicUrl") ?? "";
        var baseUrl = string.IsNullOrEmpty(publicUrl) ? "" : publicUrl.TrimEnd('/');
        var link = $"{baseUrl}/confirm-accessories?token={Uri.EscapeDataString(token)}";

        var listItems = string.Join("", all.Select(a => $"<li>{a.Item1}</li>"));
        var body = $"""
            <h1>Confirmación de accesorios - Lienzo</h1>
            <p>Tu reserva para el <strong>{reservation.Date:dd/MM/yyyy}</strong> de {reservation.StartTime:hh\:mm} a {reservation.EndTime:hh\:mm} en <strong>{classroom.Name}</strong> está pendiente de aprobación.</p>
            <p>Para que podamos confirmar la disponibilidad, marcá cuáles de los siguientes accesorios vas a necesitar:</p>
            <ul>{listItems}</ul>
            <p><a href='{link}'>Confirmar accesorios</a></p>
            <p>Si no necesitás ningún accesorio, igualmente confirmá para poder habilitar la reserva.</p>
            """;

        await _emailService.SendAsync(
            userEmail,
            $"Confirmá los accesorios para tu reserva - {classroom.Name}",
            body);
    }
}
