using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Queries.GetClase;

public record GetClaseQuery(Guid ClaseId) : IRequest<Result<ClaseResponse>>;

public class GetClaseQueryHandler : IRequestHandler<GetClaseQuery, Result<ClaseResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClaseQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClaseResponse>> Handle(GetClaseQuery request, CancellationToken ct)
    {
        var clase = await _unitOfWork.Clases.Query()
            .Include(c => c.Classroom)
            .Include(c => c.Actividad)
            .Include(c => c.Asistencias)
            .FirstOrDefaultAsync(c => c.Id == request.ClaseId && !c.IsDeleted, ct);

        if (clase is null)
            return Result<ClaseResponse>.Failure("Clase no encontrada.");

        var response = new ClaseResponse(
            clase.Id,
            clase.ReservationId,
            clase.ActividadId,
            clase.ClassroomId,
            clase.Classroom?.Name ?? "",
            clase.Actividad?.Nombre ?? "",
            clase.Fecha,
            clase.HoraInicio,
            clase.HoraFin,
            clase.Estado.ToString(),
            clase.CheckedInAt,
            clase.Asistencias.Select(a => new AsistenciaAlumnoResponse(
                a.Id,
                a.SgaAlumnoId,
                a.SgaPersonaId,
                a.AlumnoNombre,
                a.Presente,
                a.MarcadoPorUsuarioId,
                a.SgaAsistenciaId.HasValue)).ToList());

        return Result<ClaseResponse>.Success(response);
    }
}
