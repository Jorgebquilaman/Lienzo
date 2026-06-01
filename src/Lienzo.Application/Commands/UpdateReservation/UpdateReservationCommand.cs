using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.UpdateReservation;

public record UpdateReservationCommand(Guid ReservationId, UpdateReservationRequest Request) : IRequest<Result<ReservationDto>>;

public class UpdateReservationCommandHandler : IRequestHandler<UpdateReservationCommand, Result<ReservationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public UpdateReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<ReservationDto>> Handle(UpdateReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(command.ReservationId);
        if (reservation is null)
            return Result<ReservationDto>.Failure("Reservation not found", "NOT_FOUND");

        if (reservation.UserId != _currentUser.UserId)
            return Result<ReservationDto>.Failure("You can only update your own reservations", "FORBIDDEN");

        var title = command.Request.Title ?? reservation.Title;
        var description = command.Request.Description ?? reservation.Description;

        try
        {
            reservation.UpdateDetails(title, description);
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
