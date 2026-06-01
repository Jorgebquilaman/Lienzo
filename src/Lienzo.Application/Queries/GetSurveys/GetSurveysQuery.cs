using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetSurveys;

public record GetSurveysQuery(Guid? ClassroomId = null, Guid? BuildingId = null) : IRequest<Result<SurveyListResponse>>;

public class GetSurveysQueryHandler : IRequestHandler<GetSurveysQuery, Result<SurveyListResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;

    public GetSurveysQueryHandler(IUnitOfWork unitOfWork, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<Result<SurveyListResponse>> Handle(GetSurveysQuery query, CancellationToken ct)
    {
        var surveys = await _unitOfWork.ClassroomSurveys.GetAllAsync();
        var allReservations = await _unitOfWork.Reservations.GetAllAsync();
        var reservations = allReservations.ToDictionary(r => r.Id);
        var classrooms = (await _unitOfWork.Classrooms.GetAllAsync())
            .ToDictionary(c => c.Id);

        var usersResult = await _authService.GetAllUsersAsync();
        var userMap = usersResult.IsSuccess
            ? usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}")
            : new Dictionary<Guid, string>();

        var filtered = surveys.AsEnumerable();

        HashSet<Guid>? allowedReservationIds = null;

        if (query.BuildingId.HasValue)
        {
            var buildingClassroomIds = classrooms.Values
                .Where(c => c.BuildingId == query.BuildingId.Value)
                .Select(c => c.Id)
                .ToHashSet();
            allowedReservationIds = allReservations
                .Where(r => buildingClassroomIds.Contains(r.ClassroomId))
                .Select(r => r.Id)
                .ToHashSet();
        }

        if (query.ClassroomId.HasValue)
        {
            var classroomReservationIds = allReservations
                .Where(r => r.ClassroomId == query.ClassroomId.Value)
                .Select(r => r.Id)
                .ToHashSet();
            allowedReservationIds = allowedReservationIds is not null
                ? allowedReservationIds.Intersect(classroomReservationIds).ToHashSet()
                : classroomReservationIds;
        }

        if (allowedReservationIds is not null)
            filtered = filtered.Where(s => allowedReservationIds.Contains(s.ReservationId));

        var ordered = filtered.OrderByDescending(s => s.CreatedAt).ToList();

        var items = ordered.Select(s =>
        {
            reservations.TryGetValue(s.ReservationId, out var resv);
            var classroomName = "";
            if (resv is not null && classrooms.TryGetValue(resv.ClassroomId, out var cls))
                classroomName = cls.Name;
            return new SurveyDto(
                s.Id, s.ReservationId,
                userMap.GetValueOrDefault(s.UserId, s.UserId.ToString()),
                classroomName,
                s.ConditionRating, s.EquipmentRating, s.CleanlinessRating,
                s.OverallRating, s.Comment, s.CreatedAt);
        }).ToList();

        return Result<SurveyListResponse>.Success(new(items, items.Count));
    }
}
