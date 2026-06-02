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

public record DocenteCargaHorariaFilter(DateOnly? FromDate, DateOnly? ToDate, string? PeriodoId);

public record DocenteCargaHorariaItem(
    string DocenteId,
    string DocenteNombre,
    int TotalReservations,
    double TotalHoras,
    List<MesCargaHoraria> HorasPorMes);

public record MesCargaHoraria(string Mes, int Reservations, double Horas);

public record DocenteCargaHorariaResponse(
    List<DocenteCargaHorariaItem> Items,
    int TotalDocentes,
    double GranTotalHoras);

public record ClassroomTimelineFilter(DateOnly? FromDate, DateOnly? ToDate, Guid? ClassroomId, Guid? BuildingId);

public record TimelineReservationItem(
    Guid Id,
    Guid ClassroomId,
    string Title,
    DateOnly Date,
    string StartTime,
    string EndTime,
    string Status,
    string? UserName,
    string? ActividadNombre);

public record ClassroomTimelineItem(
    Guid ClassroomId,
    string ClassroomName,
    List<TimelineReservationItem> Reservations);

public record ClassroomTimelineResponse(
    List<ClassroomTimelineItem> Items,
    List<string> Dates,
    int TotalReservations);
