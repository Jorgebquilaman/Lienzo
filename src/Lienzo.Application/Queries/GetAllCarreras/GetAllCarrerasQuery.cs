using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAllCarreras;

public record GetAllCarrerasQuery : IRequest<Result<List<CarreraDto>>>;

public class GetAllCarrerasQueryHandler : IRequestHandler<GetAllCarrerasQuery, Result<List<CarreraDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetAllCarrerasQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<CarreraDto>>> Handle(GetAllCarrerasQuery query, CancellationToken ct)
    {
        var items = await _unitOfWork.Carreras.GetAllAsync();
        var dtos = items.Where(c => !c.IsDeleted).Select(c => new CarreraDto(c.Id, c.Nombre, c.Codigo)).ToList();
        return Result<List<CarreraDto>>.Success(dtos);
    }
}
