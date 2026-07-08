using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetBedeliaReport;
using Lienzo.Application.Queries.GetClassroomTimeline;
using Lienzo.Application.Queries.GetDemandMetrics;
using Lienzo.Application.Queries.GetDocenteCargaHoraria;
using Lienzo.Application.Queries.GetUsageByProposal;
using Lienzo.Application.Queries.GetUsageReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController : BaseApiController
{
    [HttpPost("usage")]
    public async Task<IActionResult> GetUsageReport([FromBody] UsageReportFilter filter)
    {
        var result = await Mediator.Send(new GetUsageReportQuery(filter));
        return HandleResult(result);
    }

    [HttpGet("demand-metrics")]
    public async Task<IActionResult> GetDemandMetrics([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        var result = await Mediator.Send(new GetDemandMetricsQuery(fromDate, toDate));
        return HandleResult(result);
    }

    [HttpPost("usage-by-proposal")]
    public async Task<IActionResult> GetUsageByProposal([FromBody] UsageByProposalFilter filter)
    {
        var result = await Mediator.Send(new GetUsageByProposalQuery(filter));
        return HandleResult(result);
    }

    [HttpPost("docente-carga-horaria")]
    public async Task<IActionResult> GetDocenteCargaHoraria([FromBody] DocenteCargaHorariaFilter filter)
    {
        var result = await Mediator.Send(new GetDocenteCargaHorariaQuery(filter));
        return HandleResult(result);
    }

    [HttpPost("classroom-timeline")]
    public async Task<IActionResult> GetClassroomTimeline([FromBody] ClassroomTimelineFilter filter)
    {
        var result = await Mediator.Send(new GetClassroomTimelineQuery(filter));
        return HandleResult(result);
    }

    [HttpGet("bedelia")]
    public async Task<IActionResult> GetBedeliaReport([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        var result = await Mediator.Send(new GetBedeliaReportQuery(fromDate, toDate));
        return HandleResult(result);
    }
}
