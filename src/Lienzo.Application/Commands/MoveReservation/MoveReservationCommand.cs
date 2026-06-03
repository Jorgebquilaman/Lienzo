using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lienzo.Application.Commands.MoveReservation;

public record MoveReservationCommand(
    Guid ReservationId,
    Guid NewClassroomId,
    bool ApplyToFuture) : IRequest<Result<bool>>;

public class MoveReservationCommandHandler : IRequestHandler<MoveReservationCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;
    private readonly INotificationService _notifications;
    private readonly ISgaAsistenciaService _sgaAsistencia;
    private readonly ILogger<MoveReservationCommandHandler> _logger;

    public MoveReservationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAuthService authService,
        INotificationService notifications,
        ISgaAsistenciaService sgaAsistencia,
        ILogger<MoveReservationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authService = authService;
        _notifications = notifications;
        _sgaAsistencia = sgaAsistencia;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(MoveReservationCommand request, CancellationToken ct)
    {
        var reservation = await _unitOfWork.Reservations.Query()
            .Include(r => r.Classroom)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId && !r.IsDeleted, ct);

        if (reservation is null)
            return Result<bool>.Failure("Reserva no encontrada.");

        if (reservation.Status != ReservationStatus.Approved)
            return Result<bool>.Failure("Solo reservas aprobadas pueden cambiarse de aula.");

        var newClassroom = await _unitOfWork.Classrooms.GetByIdAsync(request.NewClassroomId);
        if (newClassroom is null)
            return Result<bool>.Failure("Aula de destino no encontrada.");

        if (newClassroom.Id == reservation.ClassroomId)
            return Result<bool>.Failure("El aula de destino es la misma que la actual.");

        // Check for conflicts in the target classroom
        var hasConflict = await _unitOfWork.Reservations.HasConflictAsync(
            request.NewClassroomId,
            reservation.Date.ToDateTime(TimeOnly.MinValue),
            reservation.StartTime,
            reservation.EndTime,
            excludeId: reservation.Id);

        if (hasConflict)
            return Result<bool>.Failure("El aula de destino tiene un conflicto de horario en la misma fecha y hora.");

        // Apply classroom change
        var oldClassroom = reservation.Classroom;
        reservation.ChangeClassroom(request.NewClassroomId);

        // If ApplyToFuture, find all future recurring reservations and move them too
        if (request.ApplyToFuture && reservation.RecurringGroupId.HasValue)
        {
            var futureReservations = await _unitOfWork.Reservations.Query()
                .Where(r => r.RecurringGroupId == reservation.RecurringGroupId
                    && r.Date >= reservation.Date
                    && r.Id != reservation.Id
                    && !r.IsDeleted
                    && r.Status == ReservationStatus.Approved)
                .ToListAsync(ct);

            foreach (var futureReservation in futureReservations)
            {
                // Check conflict for each future date
                var futureConflict = await _unitOfWork.Reservations.HasConflictAsync(
                    request.NewClassroomId,
                    futureReservation.Date.ToDateTime(TimeOnly.MinValue),
                    futureReservation.StartTime,
                    futureReservation.EndTime,
                    excludeId: futureReservation.Id);

                if (!futureConflict)
                {
                    futureReservation.ChangeClassroom(request.NewClassroomId);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Notify the reservation owner
        var ownerMsg = $"La reserva \"{reservation.Title}\" del {reservation.Date:dd/MM/yyyy} fue movida de \"{oldClassroom.Name}\" a \"{newClassroom.Name}\".";
        await _notifications.SendAsync(
            reservation.UserId,
            "Reserva cambiada de aula",
            ownerMsg,
            "reservation_moved",
            reservation.Id,
            "Reservation");

        // If the reservation has a SGA activity, notify the students
        if (reservation.ActividadId.HasValue)
        {
            var actividad = await _unitOfWork.Actividades.GetByIdAsync(reservation.ActividadId.Value);
            if (actividad?.CodigoExterno.HasValue == true)
            {
                try
                {
                    var alumnos = await _sgaAsistencia.GetAlumnosPorComisionFechaAsync(
                        actividad.CodigoExterno.Value, reservation.Date);

                    var personaIds = alumnos.Select(a => a.PersonaId).Distinct().ToList();
                    var users = await _authService.GetUsersBySgaPersonaIdsAsync(personaIds);

                    foreach (var (userId, _) in users)
                    {
                        await _notifications.SendAsync(
                            userId,
                            "Cambio de aula",
                            $"La clase de \"{reservation.Title}\" fue movida a \"{newClassroom.Name}\".",
                            "classroom_changed",
                            reservation.Id,
                            "Reservation");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to notify students about classroom change for reservation {ReservationId}",
                        reservation.Id);
                }
            }
        }

        _logger.LogInformation(
            "Reservation {ReservationId} moved from {OldClassroom} to {NewClassroom} by user {UserId}",
            reservation.Id, oldClassroom.Name, newClassroom.Name, _currentUser.UserId);

        return Result<bool>.Success(true);
    }
}
