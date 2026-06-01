using Lienzo.Application.Common.Models;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.DeletePeriodo;

public record DeletePeriodoCommand(Guid Id) : IRequest<Result<bool>>;

public class DeletePeriodoCommandHandler : IRequestHandler<DeletePeriodoCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeletePeriodoCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeletePeriodoCommand command, CancellationToken ct)
    {
        var periodo = await _unitOfWork.Periodos.GetByIdAsync(command.Id);
        if (periodo is null) return Result<bool>.Failure("Periodo no encontrado", "NOT_FOUND");
        _unitOfWork.Periodos.Delete(periodo);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
