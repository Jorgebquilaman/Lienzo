using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetBuildingById;

public record GetBuildingByIdQuery(Guid Id) : IRequest<Result<BuildingDetailDto>>;

public class GetBuildingByIdQueryHandler : IRequestHandler<GetBuildingByIdQuery, Result<BuildingDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBuildingByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<BuildingDetailDto>> Handle(GetBuildingByIdQuery query, CancellationToken cancellationToken)
    {
        var building = await _unitOfWork.Buildings.GetWithClassroomsAsync(query.Id);
        if (building is null)
            return Result<BuildingDetailDto>.Failure("Building not found", "NOT_FOUND");

        var dto = _mapper.Map<BuildingDetailDto>(building);
        return Result<BuildingDetailDto>.Success(dto);
    }
}
