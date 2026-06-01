using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetBuildingClassrooms;

public record GetBuildingClassroomsQuery(Guid BuildingId) : IRequest<Result<List<ClassroomSummaryDto>>>;

public class GetBuildingClassroomsQueryHandler : IRequestHandler<GetBuildingClassroomsQuery, Result<List<ClassroomSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBuildingClassroomsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<ClassroomSummaryDto>>> Handle(GetBuildingClassroomsQuery query, CancellationToken cancellationToken)
    {
        var building = await _unitOfWork.Buildings.GetWithClassroomsAsync(query.BuildingId);
        if (building is null)
            return Result<List<ClassroomSummaryDto>>.Failure("Building not found", "NOT_FOUND");

        var classrooms = building.Classrooms.Where(c => !c.IsDeleted).ToList();
        var dtos = _mapper.Map<List<ClassroomSummaryDto>>(classrooms);
        return Result<List<ClassroomSummaryDto>>.Success(dtos);
    }
}
