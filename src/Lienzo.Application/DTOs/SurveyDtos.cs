namespace Lienzo.Application.DTOs;

public record CreateSurveyRequest(
    Guid ReservationId,
    decimal ConditionRating,
    decimal EquipmentRating,
    decimal CleanlinessRating,
    string? Comment);

public record SurveyDto(
    Guid Id,
    Guid ReservationId,
    string UserName,
    string ClassroomName,
    decimal ConditionRating,
    decimal EquipmentRating,
    decimal CleanlinessRating,
    decimal OverallRating,
    string? Comment,
    DateTime CreatedAt);

public record ClassroomRatingSummaryDto(
    Guid ClassroomId,
    string ClassroomName,
    decimal AverageOverall,
    decimal AverageCondition,
    decimal AverageEquipment,
    decimal AverageCleanliness,
    int TotalSurveys);

public record SurveyListResponse(List<SurveyDto> Items, int TotalCount);
