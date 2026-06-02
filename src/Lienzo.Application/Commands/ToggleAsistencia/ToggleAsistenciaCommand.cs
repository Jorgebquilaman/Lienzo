using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Commands.ToggleAsistencia;

public record ToggleAsistenciaCommand(Guid ClaseId, Guid AsistenciaId) : IRequest<Result<bool>>;

public class ToggleAsistenciaCommandHandler : IRequestHandler<ToggleAsistenciaCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ToggleAsistenciaCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(ToggleAsistenciaCommand request, CancellationToken ct)
    {
        var asistencia = await _unitOfWork.AsistenciasAlumnos.Query()
            .Include(a => a.Clase)
            .FirstOrDefaultAsync(a => a.Id == request.AsistenciaId && !a.IsDeleted, ct);

        if (asistencia is null)
            return Result<bool>.Failure("Registro de asistencia no encontrado.");

        if (asistencia.Clase.Estado != Domain.Enums.ClaseEstado.Abierta)
            return Result<bool>.Failure("La clase ya está cerrada.");

        asistencia.TogglePresencia(_currentUser.UserId);
        _unitOfWork.AsistenciasAlumnos.Update(asistencia);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(asistencia.Presente);
    }
}
