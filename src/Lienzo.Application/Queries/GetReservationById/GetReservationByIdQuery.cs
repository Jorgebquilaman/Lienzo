using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetReservationById;

public record GetReservationByIdQuery(Guid Id) : IRequest<Result<ReservationDto>>;

public class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, Result<ReservationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetReservationByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<ReservationDto>> Handle(GetReservationByIdQuery query, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(query.Id);
        if (reservation is null || reservation.IsDeleted)
            return Result<ReservationDto>.Failure("Reservation not found", "NOT_FOUND");

        if (reservation.UserId != _currentUser.UserId)
            return Result<ReservationDto>.Failure("Access denied", "FORBIDDEN");

        var dto = _mapper.Map<ReservationDto>(reservation);
        return Result<ReservationDto>.Success(dto);
    }
}
