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
    private readonly IReservationPdfGenerator _pdfGenerator;
    private readonly ILogger<ReservationCreatedEventHandler> _logger;

    public ReservationCreatedEventHandler(
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ISystemSettingService settings,
        IAuthService authService,
        IReservationPdfGenerator pdfGenerator,
        ILogger<ReservationCreatedEventHandler> logger)
    {
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _settings = settings;
        _authService = authService;
        _pdfGenerator = pdfGenerator;
        _logger = logger;
    }

    public async Task Handle(ReservationCreatedEvent notification, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Reservations.Query()
            .Include(r => r.Classroom)
            .ThenInclude(c => c.Building)
            .Include(r => r.Classroom.ClassroomAccessories)
            .ThenInclude(ca => ca.Accessory)
            .Include(r => r.ReservationAccessories)
            .FirstOrDefaultAsync(r => r.Id == notification.ReservationId && !r.IsDeleted, cancellationToken);
        if (reservation is null)
            return;

        var isApproved = reservation.Status == ReservationStatus.Approved;
        var message = isApproved
            ? $"Tu reserva para el {reservation.Date:dd/MM/yyyy} de {reservation.StartTime:hh\\:mm} a {reservation.EndTime:hh\\:mm} fue autorizada."
            : $"Tu reserva para el {reservation.Date:dd/MM/yyyy} de {reservation.StartTime:hh\\:mm} a {reservation.EndTime:hh\\:mm} está pendiente de aprobación.";

        await _notificationService.SendAsync(
            reservation.UserId,
            isApproved ? "Reserva autorizada" : "Reserva creada",
            message,
            isApproved ? "Success" : "Info",
            reservation.Id,
            "Reservation");

        try
        {
            await SendReservationEmailAsync(reservation, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reservation email for reservation {ReservationId}", reservation.Id);
        }
    }

    private async Task SendReservationEmailAsync(Domain.Entities.Reservation reservation, CancellationToken ct)
    {
        var catalogAccessories = await _unitOfWork.Accessories.Query()
            .Where(a => a.IsActive && !a.IsDeleted)
            .OrderBy(a => a.Name)
            .Select(a => new { a.Name, a.IsMovable })
            .ToListAsync(ct);

        var featureAccessories = reservation.Classroom.Features
            .Select(f => new { Name = f, IsMovable = false }).ToList();

        var allAccessories = catalogAccessories.Concat(featureAccessories).ToList();
        var hasAccessories = allAccessories.Count > 0;

        if (hasAccessories && !reservation.RequiresAccessoryConfirmation)
        {
            var token = Guid.NewGuid().ToString("N");
            var accessories = catalogAccessories
                .Select(a => (a.Name, Origin: AccessoryOrigin.Catalog))
                .Concat(featureAccessories
                    .Select(f => (f.Name, Origin: AccessoryOrigin.Feature)))
                .ToList();
            reservation.RequireAccessoryConfirmation(token, accessories);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var usersResult = await _authService.GetAllUsersAsync();
        var requester = usersResult.IsSuccess
            ? usersResult.Value.FirstOrDefault(u => u.Id == reservation.UserId)
            : null;
        var userEmail = requester?.Email;
        if (string.IsNullOrEmpty(userEmail))
        {
            _logger.LogWarning("Cannot send reservation email: user {UserId} has no email", reservation.UserId);
            return;
        }

        var userName = requester is null ? "" : $"{requester.FirstName} {requester.LastName}".Trim();
        var isApproved = reservation.Status == ReservationStatus.Approved;

        var publicUrl = await _settings.GetValueAsync("PublicUrl") ?? "";
        var baseUrl = string.IsNullOrEmpty(publicUrl) ? "" : publicUrl.TrimEnd('/');
        var confirmationLink = !string.IsNullOrEmpty(reservation.AccessoryConfirmationToken)
            ? $"{baseUrl}/confirm-accessories?token={Uri.EscapeDataString(reservation.AccessoryConfirmationToken)}"
            : null;

        string subject;
        string body;

        if (hasAccessories)
        {
            subject = $"Confirmá los accesorios para tu reserva - {reservation.Classroom.Name}";
            var listItems = string.Join("", allAccessories.Select(a => $"<li>{a.Name}</li>"));
            body = $"""
                <h1>Reserva de aula - Lienzo</h1>
                <p>Tu reserva para el <strong>{reservation.Date:dd/MM/yyyy}</strong> de {reservation.StartTime:hh\:mm} a {reservation.EndTime:hh\:mm} en <strong>{reservation.Classroom.Name}</strong> {(isApproved ? "fue autorizada" : "está pendiente de aprobación")}.</p>
                <p>Para que podamos confirmar la disponibilidad, marcá cuáles de los siguientes accesorios vas a necesitar:</p>
                <ul>{listItems}</ul>
                <p><a href='{confirmationLink}'>Confirmar accesorios</a></p>
                <p>Si no necesitás ningún accesorio, igualmente confirmá para poder habilitar la reserva.</p>
                <p>Adjuntamos un PDF con el detalle de la reserva.</p>
                """;
        }
        else
        {
            subject = isApproved
                ? $"Tu reserva fue autorizada - {reservation.Classroom.Name}"
                : $"Reserva registrada - {reservation.Classroom.Name}";
            body = $"""
                <h1>Reserva de aula - Lienzo</h1>
                <p>Tu reserva para el <strong>{reservation.Date:dd/MM/yyyy}</strong> de {reservation.StartTime:hh\:mm} a {reservation.EndTime:hh\:mm} en <strong>{reservation.Classroom.Name}</strong> {(isApproved ? "fue <strong>autorizada</strong>" : "está pendiente de aprobación")}.</p>
                <p>Adjuntamos un PDF con el detalle de la reserva.</p>
                """;
        }

        var pdf = _pdfGenerator.Generate(new Lienzo.Application.DTOs.ReservationPdfModel
        {
            Title = reservation.Title,
            Description = reservation.Description,
            UserName = userName,
            UserEmail = userEmail,
            ClassroomName = reservation.Classroom.Name,
            BuildingName = reservation.Classroom.Building?.Name,
            Floor = reservation.Classroom.Floor,
            Date = reservation.Date.ToString("dd/MM/yyyy"),
            StartTime = reservation.StartTime.ToString("hh\\:mm"),
            EndTime = reservation.EndTime.ToString("hh\\:mm"),
            Status = isApproved ? "Autorizada" : "Pendiente",
            ReservationId = reservation.Id,
            Accessories = allAccessories.Select(a => a.Name).ToList()
        });

        var attachment = new Lienzo.Application.DTOs.EmailAttachment(
            $"reserva-{reservation.Id:N}.pdf",
            "application/pdf",
            pdf);

        await _emailService.SendAsync(userEmail, subject, body, attachment);
    }
}