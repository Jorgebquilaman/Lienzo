using Lienzo.Application.Common.Models;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.DeleteReceso;

public record DeleteRecesoCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteRecesoCommandHandler : IRequestHandler<DeleteRecesoCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRecesoCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteRecesoCommand command, CancellationToken cancellationToken)
    {
        var receso = await _unitOfWork.Recesos.GetByIdAsync(command.Id);
        if (receso is null)
            return Result<bool>.Failure("Receso no encontrado", "NOT_FOUND");

        _unitOfWork.Recesos.Delete(receso);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
