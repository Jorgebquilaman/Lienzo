using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.UpdateClassroomPositions;

public record UpdateClassroomPositionsCommand(Guid BuildingId, List<ClassroomPositionDto> Positions) : IRequest<Result<bool>>;

public class UpdateClassroomPositionsCommandHandler : IRequestHandler<UpdateClassroomPositionsCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateClassroomPositionsCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(UpdateClassroomPositionsCommand command, CancellationToken cancellationToken)
    {
        var building = await _unitOfWork.Buildings.GetWithClassroomsAsync(command.BuildingId);
        if (building is null)
            return Result<bool>.Failure("Building not found", "NOT_FOUND");

        var classrooms = building.Classrooms.ToDictionary(c => c.Id);

        foreach (var pos in command.Positions)
        {
            if (classrooms.TryGetValue(pos.ClassroomId, out var classroom))
            {
                classroom.MapPositionX = pos.X;
                classroom.MapPositionY = pos.Y;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
