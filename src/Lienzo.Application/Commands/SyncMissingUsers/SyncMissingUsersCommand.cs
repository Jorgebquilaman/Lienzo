using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lienzo.Application.Commands.SyncMissingUsers;

public record SyncMissingUsersCommand(Guid ClaseId) : IRequest<Result<int>>;

public class SyncMissingUsersCommandHandler : IRequestHandler<SyncMissingUsersCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ISgaAsistenciaService _sgaAsistencia;
    private readonly ILogger<SyncMissingUsersCommandHandler> _logger;

    public SyncMissingUsersCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ISgaAsistenciaService sgaAsistencia,
        ILogger<SyncMissingUsersCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _sgaAsistencia = sgaAsistencia;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(SyncMissingUsersCommand request, CancellationToken ct)
    {
        var clase = await _unitOfWork.Clases.Query()
            .Include(c => c.Actividad)
            .FirstOrDefaultAsync(c => c.Id == request.ClaseId && !c.IsDeleted, ct);

        if (clase is null)
            return Result<int>.Failure("Clase no encontrada.");

        if (clase.SgaComisionId <= 0)
            return Result<int>.Failure("La clase no tiene comisión SGA asociada.");

        var alumnosSga = await _sgaAsistencia.GetAlumnosPorComisionFechaAsync(
            clase.SgaComisionId, clase.Fecha);

        if (alumnosSga.Count == 0)
        {
            _logger.LogWarning("No SGA students found for clase {ClaseId} (comision {Comision}, date {Date})",
                request.ClaseId, clase.SgaComisionId, clase.Fecha);
            return Result<int>.Success(0);
        }

        var beforeCount = await _authService.GetUsersBySgaPersonaIdsAsync(
            alumnosSga.Select(a => a.PersonaId).ToList());

        await _authService.EnsureStudentsExistAsync(alumnosSga);

        var afterCount = await _authService.GetUsersBySgaPersonaIdsAsync(
            alumnosSga.Select(a => a.PersonaId).ToList());

        var created = afterCount.Count - beforeCount.Count;
        _logger.LogInformation("Created {Created} missing users for clase {ClaseId}", created, request.ClaseId);

        return Result<int>.Success(created);
    }
}
