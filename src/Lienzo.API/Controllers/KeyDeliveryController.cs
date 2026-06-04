using Lienzo.Application.Commands.DeliverKey;
using Lienzo.Application.Commands.ReturnKey;
using Lienzo.Application.Commands.TransferKey;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetActiveKeyDeliveries;
using Lienzo.Application.Queries.GetKeyDeliveryHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class KeyDeliveryController : BaseApiController
{
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await Mediator.Send(new GetActiveKeyDeliveriesQuery());
        return HandleResult(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] Guid? classroomId)
    {
        var result = await Mediator.Send(new GetKeyDeliveryHistoryQuery(classroomId));
        return HandleResult(result);
    }

    [HttpPost("deliver")]
    public async Task<IActionResult> Deliver([FromBody] DeliverKeyRequest request)
    {
        var result = await Mediator.Send(new DeliverKeyCommand(request));
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid id)
    {
        var result = await Mediator.Send(new ReturnKeyCommand(id));
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/transfer")]
    public async Task<IActionResult> Transfer(Guid id, [FromBody] TransferKeyRequest request)
    {
        var result = await Mediator.Send(new TransferKeyCommand(id, request.NewUserId, request.NewUserName));
        return HandleResult(result);
    }
}

public record TransferKeyRequest(Guid NewUserId, string NewUserName);
