using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CancelReservation;

public record CancelReservationCommand(Guid ReservationId) : IRequest<Result<ReservationDto>>;

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, Result<ReservationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public CancelReservationCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<Result<ReservationDto>> Handle(CancelReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(command.ReservationId);
        if (reservation is null)
            return Result<ReservationDto>.Failure("Reservation not found", "NOT_FOUND");

        if (reservation.UserId != _currentUser.UserId)
            return Result<ReservationDto>.Failure("You can only cancel your own reservations", "FORBIDDEN");

        try
        {
            reservation.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            return Result<ReservationDto>.Failure(ex.Message, "INVALID_STATE");
        }

        _unitOfWork.Reservations.Update(reservation);

        // Find the next reservation for the same classroom/date and notify its owner
        var nextReservations = (await _unitOfWork.Reservations.GetByDateRangeAsync(
            reservation.Date, reservation.Date, classroomId: reservation.ClassroomId))
            .Where(r => r.Status == ReservationStatus.Approved
                && r.Id != reservation.Id
                && r.StartTime >= reservation.EndTime)
            .OrderBy(r => r.StartTime)
            .ToList();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var next in nextReservations)
        {
            await _notificationService.SendAsync(
                next.UserId,
                "Espacio disponible",
                $"El horario de {reservation.StartTime:hh\\:mm} a {reservation.EndTime:hh\\:mm} en {reservation.Classroom?.Name ?? "el aula"} ahora está libre. Revisa tu horario si deseas ajustar tu reserva.",
                "Info",
                reservation.Id,
                "Reservation");
        }

        var dto = _mapper.Map<ReservationDto>(reservation);
        return Result<ReservationDto>.Success(dto);
    }
}
