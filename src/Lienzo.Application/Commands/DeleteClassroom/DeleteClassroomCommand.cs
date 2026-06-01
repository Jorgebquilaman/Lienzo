using Lienzo.Application.Common.Models;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.DeleteClassroom;

public record DeleteClassroomCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteClassroomCommandHandler : IRequestHandler<DeleteClassroomCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteClassroomCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroom = await _unitOfWork.Classrooms.GetByIdAsync(command.Id);
        if (classroom is null)
            return Result<bool>.Failure("Classroom not found", "NOT_FOUND");

        classroom.Deactivate();
        classroom.IsDeleted = true;
        classroom.DeletedAt = DateTime.UtcNow;
        _unitOfWork.Classrooms.Update(classroom);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
