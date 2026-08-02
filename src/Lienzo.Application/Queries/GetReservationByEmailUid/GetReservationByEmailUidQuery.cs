using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Queries.GetReservationByEmailUid;

public record GetReservationByEmailUidQuery(string EmailUid) : IRequest<Result<ReservationDto>>;

public class GetReservationByEmailUidQueryHandler : IRequestHandler<GetReservationByEmailUidQuery, Result<ReservationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetReservationByEmailUidQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReservationDto>> Handle(GetReservationByEmailUidQuery query, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Reservations.Query()
            .FirstOrDefaultAsync(r => r.SourceEmailUid == query.EmailUid && !r.IsDeleted, cancellationToken);

        if (reservation is null)
            return Result<ReservationDto>.Failure("No se encontró una reserva para este correo.", "NOT_FOUND");

        var dto = _mapper.Map<ReservationDto>(reservation);
        dto.EvidenceFilePath = reservation.EvidenceFilePath;
        return Result<ReservationDto>.Success(dto);
    }
}
