using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.ApproveReservation;

public record ApproveReservationCommand(Guid ReservationId) : IRequest<Result<ReservationDto>>;

public class ApproveReservationCommandHandler : IRequestHandler<ApproveReservationCommand, Result<ReservationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public ApproveReservationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<ReservationDto>> Handle(ApproveReservationCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Admin.ToString())
            return Result<ReservationDto>.Failure("Only administrators can approve reservations", "FORBIDDEN");

        var reservation = await _unitOfWork.Reservations.GetByIdAsync(command.ReservationId);
        if (reservation is null)
            return Result<ReservationDto>.Failure("Reservation not found", "NOT_FOUND");

        try
        {
            reservation.Approve(_currentUser.UserId);
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
