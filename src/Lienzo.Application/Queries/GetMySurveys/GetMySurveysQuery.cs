using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetMySurveys;

public record GetMySurveysQuery : IRequest<Result<SurveyListResponse>>;

public class GetMySurveysQueryHandler : IRequestHandler<GetMySurveysQuery, Result<SurveyListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMySurveysQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<SurveyListResponse>> Handle(GetMySurveysQuery query, CancellationToken ct)
    {
        var surveys = await _unitOfWork.ClassroomSurveys.GetAllAsync();
        var allReservations = await _unitOfWork.Reservations.GetAllAsync();
        var reservations = allReservations.ToDictionary(r => r.Id);
        var classrooms = (await _unitOfWork.Classrooms.GetAllAsync())
            .ToDictionary(c => c.Id);

        var userSurveys = surveys
            .Where(s => s.UserId == _currentUser.UserId)
            .OrderByDescending(s => s.CreatedAt)
            .ToList();

        var items = userSurveys.Select(s =>
        {
            reservations.TryGetValue(s.ReservationId, out var resv);
            var classroomName = "";
            if (resv is not null && classrooms.TryGetValue(resv.ClassroomId, out var cls))
                classroomName = cls.Name;
            return new SurveyDto(
                s.Id, s.ReservationId, _currentUser.Email,
                classroomName,
                s.ConditionRating, s.EquipmentRating, s.CleanlinessRating,
                s.OverallRating, s.Comment, s.CreatedAt);
        }).ToList();

        return Result<SurveyListResponse>.Success(new(items, items.Count));
    }
}
