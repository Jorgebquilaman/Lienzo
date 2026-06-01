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
            return Result<SurveyDto>.Failure("Reservation not found", "NOT_FOUND");

        var canRate = reservation.UserId == _currentUser.UserId;
        if (!canRate && reservation.ActividadId.HasValue)
        {
            var actividad = await _unitOfWork.Actividades.GetWithDetailsAsync(reservation.ActividadId.Value);
            if (actividad is not null)
            {
                var userIdStr = _currentUser.UserId.ToString();
                canRate = actividad.Docentes.Any(d => d.DocenteId == userIdStr);
            }
        }
        if (!canRate)
            return Result<SurveyDto>.Failure("You can only rate your own reservations", "FORBIDDEN");

        var now = DateTime.UtcNow;
        var reservationEnd = reservation.Date.ToDateTime(reservation.EndTime);
        if (reservationEnd > now)
            return Result<SurveyDto>.Failure("You can only rate a reservation after it has ended", "VALIDATION");

        var existing = await _unitOfWork.ClassroomSurveys.GetAllAsync();
        if (existing.Any(s => s.ReservationId == req.ReservationId && s.UserId == _currentUser.UserId))
            return Result<SurveyDto>.Failure("You have already rated this reservation", "CONFLICT");

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
