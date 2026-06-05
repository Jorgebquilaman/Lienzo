using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.DeleteAccessory;

public record DeleteAccessoryCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteAccessoryCommandHandler : IRequestHandler<DeleteAccessoryCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccessoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteAccessoryCommand command, CancellationToken ct)
    {
        var accessory = await _unitOfWork.Accessories.GetByIdAsync(command.Id);
        if (accessory is null)
            return Result<bool>.Failure("Accesorio no encontrado", "NOT_FOUND");

        _unitOfWork.Accessories.Delete(accessory);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
