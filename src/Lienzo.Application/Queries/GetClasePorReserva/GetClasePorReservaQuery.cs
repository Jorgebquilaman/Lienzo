using Lienzo.Application.Common.Models;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Queries.GetClasePorReserva;

public record GetClasePorReservaQuery(Guid ReservationId) : IRequest<Result<Guid?>>;

public class GetClasePorReservaQueryHandler : IRequestHandler<GetClasePorReservaQuery, Result<Guid?>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClasePorReservaQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid?>> Handle(GetClasePorReservaQuery request, CancellationToken ct)
    {
        var clase = await _unitOfWork.Clases.Query()
            .Where(c => c.ReservationId == request.ReservationId
                     && c.Estado == Domain.Enums.ClaseEstado.Abierta
                     && !c.IsDeleted)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);

        return Result<Guid?>.Success(clase);
    }
}
