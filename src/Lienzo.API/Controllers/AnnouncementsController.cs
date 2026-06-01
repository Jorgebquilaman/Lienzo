using Lienzo.Application.Commands.CreateAnnouncement;
using Lienzo.Application.Commands.MarkAnnouncementAsRead;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetAnnouncements;
using Lienzo.Application.Queries.GetMyAnnouncements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize]
public class AnnouncementsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAnnouncementsQuery());
        return HandleResult(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var result = await Mediator.Send(new GetMyAnnouncementsQuery());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementRequest request)
    {
        var result = await Mediator.Send(new CreateAnnouncementCommand(request));
        return HandleResult(result);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await Mediator.Send(new MarkAnnouncementAsReadCommand(id));
        return HandleResult(result);
    }
}
