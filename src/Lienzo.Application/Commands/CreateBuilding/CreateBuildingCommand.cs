using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreateBuilding;

public record CreateBuildingCommand(CreateBuildingRequest Request) : IRequest<Result<BuildingDto>>;

public class CreateBuildingCommandHandler : IRequestHandler<CreateBuildingCommand, Result<BuildingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBuildingCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<BuildingDto>> Handle(CreateBuildingCommand command, CancellationToken cancellationToken)
    {
        var building = new Building(
            command.Request.Name,
            command.Request.Address,
            command.Request.FloorCount);

        await _unitOfWork.Buildings.AddAsync(building);
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
