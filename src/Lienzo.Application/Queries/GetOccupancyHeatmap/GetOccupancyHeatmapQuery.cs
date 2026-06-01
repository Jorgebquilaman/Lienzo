using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetOccupancyHeatmap;

public record GetOccupancyHeatmapQuery(DateTime? Date = null) : IRequest<Result<List<OccupancyHeatmapDto>>>;

public class GetOccupancyHeatmapQueryHandler : IRequestHandler<GetOccupancyHeatmapQuery, Result<List<OccupancyHeatmapDto>>>
{
    private readonly IDashboardService _dashboardService;

    public GetOccupancyHeatmapQueryHandler(IDashboardService dashboardService) => _dashboardService = dashboardService;

    public async Task<Result<List<OccupancyHeatmapDto>>> Handle(GetOccupancyHeatmapQuery query, CancellationToken cancellationToken)
        => await _dashboardService.GetOccupancyHeatmapAsync(query.Date);
}
