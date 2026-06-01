using Lienzo.Application.Common.Models;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.DeleteBuilding;

public record DeleteBuildingCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteBuildingCommandHandler : IRequestHandler<DeleteBuildingCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBuildingCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteBuildingCommand command, CancellationToken cancellationToken)
    {
        var building = await _unitOfWork.Buildings.GetByIdAsync(command.Id);
        if (building is null)
            return Result<bool>.Failure("Building not found", "NOT_FOUND");

        building.Deactivate();
        building.IsDeleted = true;
        building.DeletedAt = DateTime.UtcNow;
        _unitOfWork.Buildings.Update(building);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
