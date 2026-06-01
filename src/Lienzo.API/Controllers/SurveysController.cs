using Lienzo.Application.Commands.CreateSurvey;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetClassroomRatings;
using Lienzo.Application.Queries.GetMySurveys;
using Lienzo.Application.Queries.GetPendingSurveys;
using Lienzo.Application.Queries.GetSurveys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

public class SurveysController : BaseApiController
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSurveyRequest request)
    {
        var result = await Mediator.Send(new CreateSurveyCommand(request));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? classroomId, [FromQuery] Guid? buildingId)
    {
        var result = await Mediator.Send(new GetSurveysQuery(classroomId, buildingId));
        return HandleResult(result);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await Mediator.Send(new GetMySurveysQuery());
        return HandleResult(result);
    }

    [Authorize]
    [HttpGet("my/pending")]
    public async Task<IActionResult> GetMyPending()
    {
        var result = await Mediator.Send(new GetPendingSurveysQuery());
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("ratings")]
    public async Task<IActionResult> GetRatings([FromQuery] Guid? buildingId)
    {
        var result = await Mediator.Send(new GetClassroomRatingsQuery(buildingId));
        return HandleResult(result);
    }
}
