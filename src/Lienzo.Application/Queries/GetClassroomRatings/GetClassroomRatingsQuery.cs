using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetClassroomRatings;

public record GetClassroomRatingsQuery(Guid? BuildingId = null) : IRequest<Result<List<ClassroomRatingSummaryDto>>>;

public class GetClassroomRatingsQueryHandler : IRequestHandler<GetClassroomRatingsQuery, Result<List<ClassroomRatingSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClassroomRatingsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<ClassroomRatingSummaryDto>>> Handle(GetClassroomRatingsQuery query, CancellationToken ct)
    {
        var surveys = await _unitOfWork.ClassroomSurveys.GetAllAsync();
        var reservations = (await _unitOfWork.Reservations.GetAllAsync())
            .ToDictionary(r => r.Id);
        var classrooms = (await _unitOfWork.Classrooms.GetAllAsync())
            .ToDictionary(c => c.Id);

        var filteredSurveys = surveys.AsEnumerable();

        if (query.BuildingId.HasValue)
        {
            var buildingClassroomIds = classrooms.Values
                .Where(c => c.BuildingId == query.BuildingId.Value)
                .Select(c => c.Id)
                .ToHashSet();
            var buildingReservationIds = reservations.Values
                .Where(r => buildingClassroomIds.Contains(r.ClassroomId))
                .Select(r => r.Id)
                .ToHashSet();
            filteredSurveys = filteredSurveys.Where(s => buildingReservationIds.Contains(s.ReservationId));
        }

        var byClassroom = filteredSurveys
            .GroupBy(s =>
            {
                var resv = reservations.GetValueOrDefault(s.ReservationId);
                return resv?.ClassroomId ?? Guid.Empty;
            })
            .Select(g =>
            {
                var classroom = classrooms.GetValueOrDefault(g.Key);
                return new ClassroomRatingSummaryDto(
                    g.Key,
                    classroom?.Name ?? "Unknown",
                    Math.Round(g.Average(s => s.OverallRating), 1),
                    Math.Round(g.Average(s => s.ConditionRating), 1),
                    Math.Round(g.Average(s => s.EquipmentRating), 1),
                    Math.Round(g.Average(s => s.CleanlinessRating), 1),
                    g.Count());
            })
            .OrderByDescending(d => d.AverageOverall)
            .ToList();

        return Result<List<ClassroomRatingSummaryDto>>.Success(byClassroom);
    }
}
