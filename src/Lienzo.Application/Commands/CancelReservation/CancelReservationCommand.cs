using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CancelReservation;

public record CancelReservationCommand(Guid ReservationId) : IRequest<Result<ReservationDto>>;

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, Result<ReservationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CancelReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ReservationDto>(reservation);
        return Result<ReservationDto>.Success(dto);
    }
}
