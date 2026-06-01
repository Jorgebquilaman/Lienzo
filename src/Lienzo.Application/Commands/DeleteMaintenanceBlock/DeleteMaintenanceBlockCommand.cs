using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.DeleteMaintenanceBlock;

public record DeleteMaintenanceBlockCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteMaintenanceBlockCommandHandler : IRequestHandler<DeleteMaintenanceBlockCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteMaintenanceBlockCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(DeleteMaintenanceBlockCommand command, CancellationToken ct)
    {
        var block = await _unitOfWork.MaintenanceBlocks.GetByIdAsync(command.Id);
        if (block is null)
            return Result<bool>.Failure("Maintenance block not found", "NOT_FOUND");

        block.Deactivate();
        _unitOfWork.MaintenanceBlocks.Update(block);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
