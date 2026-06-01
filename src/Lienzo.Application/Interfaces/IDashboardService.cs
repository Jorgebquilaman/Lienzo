using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;

namespace Lienzo.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardStatsDto>> GetStatsAsync();
    Task<Result<List<OccupancyHeatmapDto>>> GetOccupancyHeatmapAsync(DateTime? date = null);
}
