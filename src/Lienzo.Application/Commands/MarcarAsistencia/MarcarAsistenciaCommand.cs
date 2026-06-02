using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Commands.MarcarAsistencia;

public record MarcarAsistenciaCommand(Guid ClaseId) : IRequest<Result<bool>>;

public class MarcarAsistenciaCommandHandler : IRequestHandler<MarcarAsistenciaCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public MarcarAsistenciaCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<Result<bool>> Handle(MarcarAsistenciaCommand request, CancellationToken ct)
    {
        var clase = await _unitOfWork.Clases.Query()
            .Include(c => c.Asistencias)
            .FirstOrDefaultAsync(c => c.Id == request.ClaseId && !c.IsDeleted, ct);

        if (clase is null)
            return Result<bool>.Failure("Clase no encontrada.");

        if (clase.Estado != Domain.Enums.ClaseEstado.Abierta)
            return Result<bool>.Failure("La clase ya está cerrada.");

        var sgaPersonaId = await _authService.GetSgaPersonaIdAsync(_currentUser.UserId);
        if (!sgaPersonaId.HasValue)
            return Result<bool>.Failure("Tu cuenta no está vinculada a un alumno. Contactá al administrador.");

        var asistencia = clase.Asistencias
            .FirstOrDefault(a => a.SgaPersonaId == sgaPersonaId.Value && !a.IsDeleted);

        if (asistencia is null)
            return Result<bool>.Failure("No estás inscripto en esta clase.");

        if (asistencia.Presente)
            return Result<bool>.Failure("Ya marcaste tu asistencia.");

        asistencia.MarcarPresente(_currentUser.UserId);
        _unitOfWork.AsistenciasAlumnos.Update(asistencia);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
