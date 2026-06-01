using Lienzo.Application.Common.Models;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.DeleteActividad;

public record DeleteActividadCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteActividadCommandHandler : IRequestHandler<DeleteActividadCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteActividadCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteActividadCommand command, CancellationToken ct)
    {
        var actividad = await _unitOfWork.Actividades.GetByIdAsync(command.Id);
        if (actividad is null) return Result<bool>.Failure("Actividad no encontrada", "NOT_FOUND");
        _unitOfWork.Actividades.Delete(actividad);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
