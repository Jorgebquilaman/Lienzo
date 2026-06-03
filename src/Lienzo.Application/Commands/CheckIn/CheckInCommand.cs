using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lienzo.Application.Commands.CheckIn;

public record CheckInCommand(Guid ReservationId) : IRequest<Result<CheckInResult>>;

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result<CheckInResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;
    private readonly ISgaAsistenciaService _sgaAsistencia;
    private readonly ILogger<CheckInCommandHandler> _logger;

    public CheckInCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuthService authService,
        ISgaAsistenciaService sgaAsistencia,
        ILogger<CheckInCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authService = authService;
        _sgaAsistencia = sgaAsistencia;
        _logger = logger;
    }

    public async Task<Result<CheckInResult>> Handle(CheckInCommand request, CancellationToken ct)
    {
        var reservation = await _unitOfWork.Reservations.Query()
            .Include(r => r.Classroom)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId && !r.IsDeleted, ct);

        if (reservation is null)
            return Result<CheckInResult>.Failure("Reserva no encontrada.");

        if (reservation.Status != Domain.Enums.ReservationStatus.Approved)
            return Result<CheckInResult>.Failure("La reserva no está aprobada.");

        if (!reservation.ActividadId.HasValue)
            return Result<CheckInResult>.Failure("La reserva no tiene una actividad asociada.");

        var actividad = await _unitOfWork.Actividades.GetByIdAsync(reservation.ActividadId.Value);
        if (actividad is null)
            return Result<CheckInResult>.Failure("Actividad no encontrada.");

        if (!actividad.CodigoExterno.HasValue)
            return Result<CheckInResult>.Failure("La actividad no tiene código externo (comisión SGA).");

        // Check if already checked in for this reservation
        var existingClases = await _unitOfWork.Clases.Query()
            .Where(c => c.ReservationId == request.ReservationId && !c.IsDeleted)
            .ToListAsync(ct);

        if (existingClases.Any(c => c.Estado == Domain.Enums.ClaseEstado.Abierta))
            return Result<CheckInResult>.Failure("Ya hay un check-in activo para esta reserva.");

        // Get students from SGA
        var alumnosSga = await _sgaAsistencia.GetAlumnosPorComisionFechaAsync(
            actividad.CodigoExterno.Value, reservation.Date);

        if (alumnosSga.Count == 0)
            return Result<CheckInResult>.Failure("No se encontraron alumnos inscriptos en SGA para esta clase.");

        // Ensure all SGA students have an ApplicationUser account so they can log in
        await _authService.EnsureStudentsExistAsync(alumnosSga);

        // Create Clase
        int? claseSgaClaseId = alumnosSga[0].SgaClaseId > 0 ? alumnosSga[0].SgaClaseId : null;
        var clase = new Clase(
            reservation.Id,
            reservation.ActividadId.Value,
            reservation.ClassroomId,
            reservation.Date,
            reservation.StartTime,
            reservation.EndTime,
            actividad.CodigoExterno.Value,
            claseSgaClaseId,
            _currentUser.UserId);

        // Create AsistenciaAlumno for each SGA student
        foreach (var alumno in alumnosSga)
        {
            var nombre = $"{alumno.Apellido}, {alumno.Nombres}".Trim(',', ' ');
            var asistencia = new AsistenciaAlumno(
                clase.Id,
                alumno.AlumnoId,
                alumno.PersonaId,
                nombre,
                "");
            clase.AgregarAsistencia(asistencia);
        }

        await _unitOfWork.Clases.AddAsync(clase);
        await _unitOfWork.SaveChangesAsync(ct);

        var qrUrl = $"/asistencia/marcar?claseId={clase.Id}";

        _logger.LogInformation("Check-in created for reservation {ReservationId}, clase {ClaseId}, {Alumnos} alumnos",
            request.ReservationId, clase.Id, alumnosSga.Count);

        return Result<CheckInResult>.Success(new CheckInResult(
            clase.Id,
            qrUrl,
            alumnosSga.Count));
    }
}
