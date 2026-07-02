using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.SetBuildingFloorPlan;

public record SetBuildingFloorPlanCommand(Guid BuildingId, string? FloorPlanUrl) : IRequest<Result<BuildingDto>>;

public class SetBuildingFloorPlanCommandHandler : IRequestHandler<SetBuildingFloorPlanCommand, Result<BuildingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetBuildingFloorPlanCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<BuildingDto>> Handle(SetBuildingFloorPlanCommand command, CancellationToken cancellationToken)
    {
        var building = await _unitOfWork.Buildings.GetByIdAsync(command.BuildingId);
        if (building is null)
            return Result<BuildingDto>.Failure("Building not found", "NOT_FOUND");

        building.SetFloorPlanUrl(command.FloorPlanUrl);
        _unitOfWork.Buildings.Update(building);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BuildingDto>.Success(new BuildingDto(
            building.Id,
            building.Name,
            building.Address,
            building.FloorCount,
            building.IsActive,
            building.CodigoExterno,
            building.CreatedAt,
            building.FloorPlanUrl));
    }
}
