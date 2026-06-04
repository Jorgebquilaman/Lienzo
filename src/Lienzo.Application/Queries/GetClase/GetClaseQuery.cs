using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Application.Queries.GetClase;

public record GetClaseQuery(Guid ClaseId) : IRequest<Result<ClaseResponse>>;

public class GetClaseQueryHandler : IRequestHandler<GetClaseQuery, Result<ClaseResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public GetClaseQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authService = authService;
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

        string? alumnoNombre = null;
        var sgaPersonaId = await _authService.GetSgaPersonaIdAsync(_currentUser.UserId);
        if (sgaPersonaId.HasValue)
        {
            var asistencia = clase.Asistencias
                .FirstOrDefault(a => a.SgaPersonaId == sgaPersonaId.Value && !a.IsDeleted);
            if (asistencia is not null)
                alumnoNombre = asistencia.AlumnoNombre;
        }

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
            alumnoNombre,
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
