using Lienzo.Application.Commands.MarkAllNotificationsAsRead;
using Lienzo.Application.Commands.MarkNotificationAsRead;
using Lienzo.Application.Queries.GetNotifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize]
public class NotificationsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetNotificationsQuery());
        return HandleResult(result);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await Mediator.Send(new MarkNotificationAsReadCommand(id));
        return HandleResult(result);
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await Mediator.Send(new MarkAllNotificationsAsReadCommand());
        return HandleResult(result);
    }
}
