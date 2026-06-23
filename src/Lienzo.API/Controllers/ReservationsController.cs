using Lienzo.Application.Commands.ApproveReservation;
using Lienzo.Application.Commands.CancelReservation;
using Lienzo.Application.Commands.CreateReservation;
using Lienzo.Application.Commands.MoveReservation;
using Lienzo.Application.Commands.RejectReservation;
using Lienzo.Application.Commands.UpdateReservation;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetAllReservations;
using Lienzo.Application.Queries.GetReservationById;
using Lienzo.Application.Queries.GetSchedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize]
public class ReservationsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, [FromQuery] string? filter = null)
    {
        var result = await Mediator.Send(new GetAllReservationsQuery(page, pageSize, status, filter));
        return HandleResult(result);
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest request)
    {
        var result = await Mediator.Send(new CreateReservationCommand(request));
        return HandleResult(result);
    }

    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] Guid? buildingId = null,
        [FromQuery] Guid? classroomId = null)
    {
        var result = await Mediator.Send(new GetScheduleQuery(fromDate, toDate, buildingId, classroomId));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetReservationByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReservationRequest request)
    {
        var result = await Mediator.Send(new UpdateReservationCommand(id, request));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await Mediator.Send(new CancelReservationCommand(id));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await Mediator.Send(new ApproveReservationCommand(id));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await Mediator.Send(new RejectReservationCommand(id));
        return HandleResult(result);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelByUser(Guid id)
    {
        var result = await Mediator.Send(new CancelReservationCommand(id));
        return HandleResult(result);
    }

    [HttpPatch("{id:guid}/cancel-future")]
    public async Task<IActionResult> CancelFuture(Guid id)
    {
        var result = await Mediator.Send(new CancelRecurringFutureCommand(id));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/move")]
    public async Task<IActionResult> Move(Guid id, [FromBody] MoveReservationRequest request)
    {
        var result = await Mediator.Send(new MoveReservationCommand(id, request.NewClassroomId, request.ApplyToFuture));
        return HandleResult(result);
    }

    public record MoveReservationRequest(Guid NewClassroomId, bool ApplyToFuture);
}
