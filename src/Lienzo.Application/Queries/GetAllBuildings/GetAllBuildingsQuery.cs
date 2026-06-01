using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAllBuildings;

public record GetAllBuildingsQuery : IRequest<Result<List<BuildingDto>>>;

public class GetAllBuildingsQueryHandler : IRequestHandler<GetAllBuildingsQuery, Result<List<BuildingDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllBuildingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<BuildingDto>>> Handle(GetAllBuildingsQuery query, CancellationToken cancellationToken)
    {
        var buildings = await _unitOfWork.Buildings.GetAllAsync();
        var buildingDtos = _mapper.Map<List<BuildingDto>>(buildings.ToList());
        return Result<List<BuildingDto>>.Success(buildingDtos);
    }
}
