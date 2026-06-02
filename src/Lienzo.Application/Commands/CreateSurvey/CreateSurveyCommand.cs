using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreateSurvey;

public record CreateSurveyCommand(CreateSurveyRequest Request) : IRequest<Result<SurveyDto>>;

public class CreateSurveyCommandHandler : IRequestHandler<CreateSurveyCommand, Result<SurveyDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateSurveyCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<SurveyDto>> Handle(CreateSurveyCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var reservation = await _unitOfWork.Reservations.GetByIdAsync(req.ReservationId);
        if (reservation is null)
            return Result<SurveyDto>.Failure("Reservación no encontrada", "NOT_FOUND");

        if (!reservation.ActividadId.HasValue)
            return Result<SurveyDto>.Failure(
                "Esta reservación no está vinculada a una actividad académica, por lo que no se puede evaluar",
                "FORBIDDEN");

        var actividad = await _unitOfWork.Actividades.GetWithDetailsAsync(reservation.ActividadId.Value);
        if (actividad is null)
            return Result<SurveyDto>.Failure("La actividad académica vinculada no existe", "NOT_FOUND");

        var userIdStr = _currentUser.UserId.ToString();
        var esDocente = actividad.Docentes.Any(d => d.DocenteId == userIdStr);
        if (!esDocente)
            return Result<SurveyDto>.Failure(
                "Solo el docente asignado a la actividad puede evaluar esta reservación",
                "FORBIDDEN");

        var now = DateTime.UtcNow;
        var reservationEnd = reservation.Date.ToDateTime(reservation.EndTime);
        if (reservationEnd > now)
            return Result<SurveyDto>.Failure(
                "Solo puedes evaluar una reservación después de que haya finalizado",
                "VALIDATION");

        var existing = await _unitOfWork.ClassroomSurveys.GetAllAsync();
        if (existing.Any(s => s.ReservationId == req.ReservationId && s.UserId == _currentUser.UserId))
            return Result<SurveyDto>.Failure("Ya has evaluado esta reservación", "CONFLICT");

        ClassroomSurvey survey;
        try
        {
            survey = new ClassroomSurvey(
                req.ReservationId,
                _currentUser.UserId,
                req.ConditionRating,
                req.EquipmentRating,
                req.CleanlinessRating,
                req.Comment);
        }
        catch (ArgumentException ex)
        {
            return Result<SurveyDto>.Failure(ex.Message, "VALIDATION");
        }

        await _unitOfWork.ClassroomSurveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync(ct);

        var classroomName = reservation.Classroom?.Name ?? "";

        var dto = new SurveyDto(
            survey.Id, survey.ReservationId, _currentUser.Email, classroomName,
            survey.ConditionRating, survey.EquipmentRating, survey.CleanlinessRating,
            survey.OverallRating, survey.Comment, survey.CreatedAt);

        return Result<SurveyDto>.Success(dto);
    }
}
