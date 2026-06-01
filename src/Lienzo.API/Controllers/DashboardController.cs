using Lienzo.Application.Queries.GetDashboardStats;
using Lienzo.Application.Queries.GetOccupancyHeatmap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class DashboardController : BaseApiController
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await Mediator.Send(new GetDashboardStatsQuery());
        return HandleResult(result);
    }

    [HttpGet("occupancy-heatmap")]
    public async Task<IActionResult> GetOccupancyHeatmap([FromQuery] DateTime? date)
    {
        var result = await Mediator.Send(new GetOccupancyHeatmapQuery(date));
        return HandleResult(result);
    }
}
