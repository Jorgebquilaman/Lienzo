using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Commands.SyncSga;

public record SyncSgaCommand(Guid ClaseId) : IRequest<Result<SyncResult>>;

public class SyncSgaCommandHandler : IRequestHandler<SyncSgaCommand, Result<SyncResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISgaAsistenciaService _sgaAsistencia;

    public SyncSgaCommandHandler(IUnitOfWork unitOfWork, ISgaAsistenciaService sgaAsistencia)
    {
        _unitOfWork = unitOfWork;
        _sgaAsistencia = sgaAsistencia;
    }

    public async Task<Result<SyncResult>> Handle(SyncSgaCommand request, CancellationToken ct)
    {
        var clase = await _unitOfWork.Clases.Query()
            .Include(c => c.Asistencias)
            .FirstOrDefaultAsync(c => c.Id == request.ClaseId && !c.IsDeleted, ct);

        if (clase is null)
            return Result<SyncResult>.Failure("Clase no encontrada.");

        if (!clase.SgaClaseId.HasValue)
            return Result<SyncResult>.Failure("La clase no tiene un ID de SGA asociado.");

        var toSync = clase.Asistencias
            .Where(a => !a.IsDeleted)
            .Select(a => (a.SgaAlumnoId, a.Presente))
            .ToList();

        if (toSync.Count == 0)
            return Result<SyncResult>.Failure("No hay registros para sincronizar.");

        var syncResult = await _sgaAsistencia.SincronizarAsistenciaAsync(clase.SgaClaseId.Value, toSync);

        clase.Cerrar();
        _unitOfWork.Clases.Update(clase);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<SyncResult>.Success(new SyncResult(
            syncResult.Actualizados,
            syncResult.Errores,
            syncResult.DetalleErrores));
    }
}
