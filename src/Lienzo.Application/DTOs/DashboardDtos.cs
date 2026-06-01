namespace Lienzo.Application.DTOs;

public record DashboardStatsDto(int TotalClassrooms, int ActiveClassrooms, int ReservationsToday, int PendingApprovals, double OccupancyRate, int TotalUsers);

public record OccupancyHeatmapDto(string Day, int Hour, int ReservationsCount);
