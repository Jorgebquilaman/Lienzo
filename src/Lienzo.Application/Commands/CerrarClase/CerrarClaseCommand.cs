using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Commands.CerrarClase;

public record CerrarClaseCommand(Guid ClaseId) : IRequest<Result<bool>>;

public class CerrarClaseCommandHandler : IRequestHandler<CerrarClaseCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public CerrarClaseCommandHandler(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> Handle(CerrarClaseCommand request, CancellationToken ct)
    {
        var clase = await _unitOfWork.Clases.Query()
            .Include(c => c.Reservation)
                .ThenInclude(r => r.Classroom)
            .FirstOrDefaultAsync(c => c.Id == request.ClaseId && !c.IsDeleted, ct);

        if (clase is null)
            return Result<bool>.Failure("Clase no encontrada.");

        if (clase.Estado != Domain.Enums.ClaseEstado.Abierta)
            return Result<bool>.Failure("La clase ya está cerrada.");

        clase.Cerrar();
        _unitOfWork.Clases.Update(clase);
        await _unitOfWork.SaveChangesAsync(ct);

        var reservation = clase.Reservation;
        if (reservation is not null)
        {
            await _notificationService.SendAsync(
                reservation.UserId,
                "Encuesta de aula",
                $"La clase en {reservation.Classroom?.Name ?? "el aula"} ha finalizado. Por favor completá la encuesta en Mis Encuestas.",
                "Info",
                reservation.Id,
                "Reservation");
        }

        return Result<bool>.Success(true);
    }
}
