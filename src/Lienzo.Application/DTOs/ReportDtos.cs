namespace Lienzo.Application.DTOs;

public record UsageReportFilter(
    DateOnly? FromDate,
    DateOnly? ToDate,
    Guid? CarreraId,
    string? DocenteId,
    Guid? ClassroomId);

public record UsageReportItem(
    string Group,
    int TotalReservations,
    int ApprovedReservations,
    int CancelledReservations,
    int NoShowReservations,
    double TotalHours,
    double CancellationRate,
    double UsagePercentage);

public record UsageReportResponse(
    List<UsageReportItem> Items,
    int GrandTotalReservations,
    double GrandTotalHours,
    double OverallCancellationRate);

public record DemandMetricItem(
    int Hour,
    string ClassroomType,
    int ReservationCount,
    double OccupancyPercentage);

public record DemandMetricsResponse(
    List<DemandMetricItem> Items,
    List<DemandMetricItem> PeakHours,
    List<ClassroomDemandSummary> ByClassroomType);

public record ClassroomDemandSummary(
    string ClassroomType,
    int TotalReservations,
    double TotalHours,
    double UtilizationRate);

public record UsageByProposalFilter(
    DateOnly? FromDate,
    DateOnly? ToDate,
    string GroupBy);

public record UsageByProposalItem(
    string Group,
    int TotalReservations,
    int ApprovedReservations,
    int CancelledReservations,
    double TotalHours,
    double CancellationRate,
    double UsagePercentage);

public record UsageByProposalResponse(
    List<UsageByProposalItem> Items,
    int GrandTotalReservations,
    double GrandTotalHours,
    double OverallCancellationRate);
