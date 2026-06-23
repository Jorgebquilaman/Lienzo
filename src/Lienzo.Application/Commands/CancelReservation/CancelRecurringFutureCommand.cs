using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Commands.CancelReservation;

public record CancelRecurringFutureCommand(Guid ReservationId) : IRequest<Result<List<ReservationDto>>>;

public class CancelRecurringFutureCommandHandler : IRequestHandler<CancelRecurringFutureCommand, Result<List<ReservationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CancelRecurringFutureCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ReservationDto>>> Handle(CancelRecurringFutureCommand command, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(command.ReservationId);
        if (reservation is null)
            return Result<List<ReservationDto>>.Failure("Reservation not found", "NOT_FOUND");

        if (reservation.UserId != _currentUser.UserId)
            return Result<List<ReservationDto>>.Failure("You can only cancel your own reservations", "FORBIDDEN");

        if (!reservation.RecurringGroupId.HasValue)
            return Result<List<ReservationDto>>.Failure("Reservation is not recurring", "INVALID_STATE");

        var futureReservations = await _unitOfWork.Reservations.Query()
            .Where(r => r.RecurringGroupId == reservation.RecurringGroupId
                && r.Date >= reservation.Date
                && !r.IsDeleted
                && r.Status != ReservationStatus.Cancelled
                && r.Status != ReservationStatus.Rejected)
            .ToListAsync(cancellationToken);

        var cancelled = new List<Reservation>();
        foreach (var r in futureReservations)
        {
            try
            {
                r.Cancel();
                _unitOfWork.Reservations.Update(r);
                cancelled.Add(r);
            }
            catch (InvalidOperationException)
            {
                // Skip already-cancelled/rejected
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dtos = _mapper.Map<List<ReservationDto>>(cancelled);
        return Result<List<ReservationDto>>.Success(dtos);
    }
}
