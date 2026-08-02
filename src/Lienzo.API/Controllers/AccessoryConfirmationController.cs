using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccessoryConfirmationController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AccessoryConfirmationController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> Get(string token)
    {
        var reservation = await _unitOfWork.Reservations.Query()
            .Include(r => r.Classroom)
            .Include(r => r.ReservationAccessories)
            .FirstOrDefaultAsync(r => r.AccessoryConfirmationToken == token && !r.IsDeleted);

        if (reservation is null)
            return NotFound(new ProblemDetails { Title = "Enlace inválido o expirado.", Status = 404 });

        var dto = new AccessoryConfirmationDto(
            token,
            reservation.Classroom.Name,
            reservation.Date.ToDateTime(TimeOnly.MinValue),
            reservation.StartTime,
            reservation.EndTime,
            reservation.Title,
            reservation.AccessoriesConfirmedAt.HasValue,
            reservation.ReservationAccessories
                .Select(a => new AccessoryConfirmationOptionDto(a.Name, a.Origin.ToString()))
                .ToList());

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Confirm([FromBody] ConfirmAccessoriesRequest request)
    {
        var reservation = await _unitOfWork.Reservations.Query()
            .Include(r => r.ReservationAccessories)
            .FirstOrDefaultAsync(r => r.AccessoryConfirmationToken == request.Token && !r.IsDeleted);

        if (reservation is null)
            return NotFound(new ProblemDetails { Title = "Enlace inválido o expirado.", Status = 404 });

        if (reservation.AccessoriesConfirmedAt.HasValue)
            return BadRequest(new ProblemDetails { Title = "Este enlace ya fue utilizado para confirmar los accesorios.", Status = 400 });

        var validNames = reservation.ReservationAccessories.Select(a => a.Name).ToHashSet();
        var requested = request.RequestedAccessories
            .Where(n => validNames.Contains(n))
            .Distinct()
            .ToList();

        try
        {
            reservation.ConfirmAccessories(requested);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = 400 });
        }

        _unitOfWork.Reservations.Update(reservation);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Accesorios confirmados correctamente. Tu reserva ya puede ser aprobada." });
    }
}
