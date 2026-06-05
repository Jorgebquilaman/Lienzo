using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Queries.GetAllAccessories;

public record GetAllAccessoriesQuery : IRequest<Result<List<AccessoryDto>>>;

public class GetAllAccessoriesQueryHandler : IRequestHandler<GetAllAccessoriesQuery, Result<List<AccessoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllAccessoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<AccessoryDto>>> Handle(GetAllAccessoriesQuery query, CancellationToken ct)
    {
        var accessories = await _unitOfWork.Accessories.Query()
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

        var dtos = accessories.Select(a => new AccessoryDto(a.Id, a.Name, a.Description, a.IsActive)).ToList();
        return Result<List<AccessoryDto>>.Success(dtos);
    }
}
