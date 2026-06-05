using Lienzo.Application.Queries.GetCampusStatus;
using Lienzo.Application.Queries.GetSchedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[AllowAnonymous]
public class PublicController : BaseApiController
{
    [HttpGet("campus-status")]
    public async Task<IActionResult> GetCampusStatus()
    {
        var result = await Mediator.Send(new GetCampusStatusQuery());
        return HandleResult(result);
    }

    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        var result = await Mediator.Send(new GetScheduleQuery(fromDate, toDate));
        return HandleResult(result);
    }
}
