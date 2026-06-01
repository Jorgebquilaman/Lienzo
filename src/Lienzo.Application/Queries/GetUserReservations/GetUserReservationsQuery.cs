using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetUserReservations;

public record GetUserReservationsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PaginatedResult<ReservationDto>>>;

public class GetUserReservationsQueryHandler : IRequestHandler<GetUserReservationsQuery, Result<PaginatedResult<ReservationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetUserReservationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedResult<ReservationDto>>> Handle(GetUserReservationsQuery query, CancellationToken cancellationToken)
    {
        var reservations = await _unitOfWork.Reservations.GetUserReservationsAsync(_currentUser.UserId);
        var filtered = reservations.Where(r => !r.IsDeleted).ToList();
        var totalCount = filtered.Count;

        var paged = filtered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<ReservationDto>>(paged);

        return Result<PaginatedResult<ReservationDto>>.Success(
            PaginatedResult<ReservationDto>.Success(dtos, totalCount, query.Page, query.PageSize));
    }
}
