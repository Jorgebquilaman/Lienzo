using Lienzo.Application.Common.Models;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.DeleteCarrera;

public record DeleteCarreraCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteCarreraCommandHandler : IRequestHandler<DeleteCarreraCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteCarreraCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteCarreraCommand command, CancellationToken ct)
    {
        var carrera = await _unitOfWork.Carreras.GetByIdAsync(command.Id);
        if (carrera is null) return Result<bool>.Failure("Carrera no encontrada", "NOT_FOUND");
        _unitOfWork.Carreras.Delete(carrera);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
