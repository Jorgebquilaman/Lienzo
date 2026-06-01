using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.UpdateBuilding;

public record UpdateBuildingCommand(Guid Id, UpdateBuildingRequest Request) : IRequest<Result<BuildingDto>>;

public class UpdateBuildingCommandHandler : IRequestHandler<UpdateBuildingCommand, Result<BuildingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBuildingCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<BuildingDto>> Handle(UpdateBuildingCommand command, CancellationToken cancellationToken)
    {
        var building = await _unitOfWork.Buildings.GetByIdAsync(command.Id);
        if (building is null)
            return Result<BuildingDto>.Failure("Building not found", "NOT_FOUND");

        var request = command.Request;
        var name = request.Name ?? building.Name;
        var address = request.Address ?? building.Address;
        var floorCount = request.FloorCount ?? building.FloorCount;

        building.UpdateDetails(name, address, floorCount);
        _unitOfWork.Buildings.Update(building);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BuildingDto>.Success(new BuildingDto(
            building.Id,
            building.Name,
            building.Address,
            building.FloorCount,
            building.IsActive,
            building.CodigoExterno,
            building.CreatedAt));
    }
}
