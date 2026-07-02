using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAllRecesos;

public record GetAllRecesosQuery : IRequest<Result<List<RecesoDto>>>;

public class GetAllRecesosQueryHandler : IRequestHandler<GetAllRecesosQuery, Result<List<RecesoDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllRecesosQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<RecesoDto>>> Handle(GetAllRecesosQuery query, CancellationToken cancellationToken)
    {
        var recesos = await _unitOfWork.Recesos.GetAllAsync();
        var dtos = recesos.Select(r => new RecesoDto(
            r.Id,
            r.StartDate.ToString("yyyy-MM-dd"),
            r.EndDate.ToString("yyyy-MM-dd"),
            r.Description)).ToList();
        return Result<List<RecesoDto>>.Success(dtos);
    }
}
