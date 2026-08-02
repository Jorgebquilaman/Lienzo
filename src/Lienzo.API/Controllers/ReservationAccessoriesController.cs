using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/reservations/{reservationId:guid}/accessories")]
public class ReservationAccessoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ISystemSettingService _settings;
    private readonly IAuthService _authService;
    private readonly IReservationPdfGenerator _pdfGenerator;

    public ReservationAccessoriesController(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ISystemSettingService settings,
        IAuthService authService,
        IReservationPdfGenerator pdfGenerator)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _settings = settings;
        _authService = authService;
        _pdfGenerator = pdfGenerator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAccessories(Guid reservationId)
    {
        var reservation = await _unitOfWork.Reservations.Query()
            .Include(r => r.Classroom)
            .Include(r => r.ReservationAccessories)
            .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

        if (reservation is null)
            return NotFound(new ProblemDetails { Title = "Reserva no encontrada.", Status = 404 });

        var dto = new ReservationAccessoriesDto(
            reservation.Id,
            reservation.RequiresAccessoryConfirmation,
            reservation.AccessoriesConfirmedAt.HasValue,
            reservation.ReservationAccessories
                .Select(a => new ReservationAccessoryDto(a.Name, a.Origin.ToString(), a.IsRequested, a.IsGranted))
                .ToList());

        return Ok(dto);
    }

    [HttpPost("decide")]
    public async Task<IActionResult> Decide(Guid reservationId, [FromBody] List<DecideAccessoryRequest> decisions)
    {
        var reservation = await _unitOfWork.Reservations.Query()
            .Include(r => r.ReservationAccessories)
            .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

        if (reservation is null)
            return NotFound(new ProblemDetails { Title = "Reserva no encontrada.", Status = 404 });

        if (!reservation.AccessoriesConfirmedAt.HasValue)
            return BadRequest(new ProblemDetails { Title = "El solicitante aún no confirmó los accesorios.", Status = 400 });

        try
        {
            foreach (var decision in decisions)
                reservation.DecideAccessory(decision.Name, decision.Granted);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = 400 });
        }

        _unitOfWork.Reservations.Update(reservation);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Decisiones de accesorios guardadas correctamente." });
    }

    [HttpPost("resend")]
    public async Task<IActionResult> Resend(Guid reservationId)
    {
        var reservation = await _unitOfWork.Reservations.Query()
            .Include(r => r.Classroom)
            .ThenInclude(c => c.Building)
            .Include(r => r.Classroom.ClassroomAccessories)
            .ThenInclude(ca => ca.Accessory)
            .Include(r => r.ReservationAccessories)
            .FirstOrDefaultAsync(r => r.Id == reservationId && !r.IsDeleted);

        if (reservation is null)
            return NotFound(new ProblemDetails { Title = "Reserva no encontrada.", Status = 404 });

        if (reservation.AccessoriesConfirmedAt.HasValue)
            return BadRequest(new ProblemDetails { Title = "La confirmación ya fue recibida.", Status = 400 });

        if (!reservation.RequiresAccessoryConfirmation)
        {
            var classroomAccessories = reservation.Classroom.ClassroomAccessories
                .Where(ca => ca.Accessory is { IsActive: true })
                .Select(ca => (ca.Accessory.Name, Origin: Lienzo.Domain.Enums.AccessoryOrigin.Catalog))
                .ToList();

            var featureAccessories = reservation.Classroom.Features
                .Select(f => (f, Origin: Lienzo.Domain.Enums.AccessoryOrigin.Feature))
                .ToList();

            var allAccessories = classroomAccessories.Concat(featureAccessories).ToList();
            if (allAccessories.Count == 0)
                return BadRequest(new ProblemDetails { Title = "El aula no tiene accesorios configurados. Asigná accesorios al aula para poder enviar la confirmación.", Status = 400 });

            reservation.RequireAccessoryConfirmation(Guid.NewGuid().ToString("N"), allAccessories);
            await _unitOfWork.SaveChangesAsync();
        }

        try
        {
            var usersResult = await _authService.GetAllUsersAsync();
            var requester = usersResult.IsSuccess
                ? usersResult.Value.FirstOrDefault(u => u.Id == reservation.UserId)
                : null;
            var userEmail = requester?.Email;
            if (string.IsNullOrEmpty(userEmail))
                return BadRequest(new ProblemDetails { Title = "No se pudo obtener el correo del solicitante.", Status = 400 });

            var userName = requester is null ? "" : $"{requester.FirstName} {requester.LastName}".Trim();

            var publicUrl = await _settings.GetValueAsync("PublicUrl") ?? "";
            var baseUrl = string.IsNullOrEmpty(publicUrl) ? "" : publicUrl.TrimEnd('/');
            var link = $"{baseUrl}/confirm-accessories?token={Uri.EscapeDataString(reservation.AccessoryConfirmationToken!)}";

            var listItems = string.Join("", reservation.ReservationAccessories.Select(a => $"<li>{a.Name}</li>"));
            var body = $"""
                <h1>Confirmación de accesorios - Lienzo</h1>
                <p>Tu reserva para el <strong>{reservation.Date:dd/MM/yyyy}</strong> de {reservation.StartTime:hh\:mm} a {reservation.EndTime:hh\:mm} en <strong>{reservation.Classroom.Name}</strong> está pendiente de aprobación.</p>
                <p>Para que podamos confirmar la disponibilidad, marcá cuáles de los siguientes accesorios vas a necesitar:</p>
                <ul>{listItems}</ul>
                <p><a href='{link}'>Confirmar accesorios</a></p>
                <p>Si no necesitás ningún accesorio, igualmente confirmá para poder habilitar la reserva.</p>
                <p>Adjuntamos un PDF con el detalle de la reserva.</p>
                """;

            var pdf = _pdfGenerator.Generate(new ReservationPdfModel
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
                Status = "Pendiente",
                ReservationId = reservation.Id,
                Accessories = reservation.ReservationAccessories.Select(a => a.Name).ToList()
            });

            var attachment = new EmailAttachment($"reserva-{reservation.Id:N}.pdf", "application/pdf", pdf);
            await _emailService.SendAsync(userEmail, $"Confirmá los accesorios para tu reserva - {reservation.Classroom.Name}", body, attachment);
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails { Title = $"Error al enviar el correo: {ex.Message}", Status = 400 });
        }

        return Ok(new { message = "Correo de confirmación reenviado correctamente." });
    }
}
