using Lienzo.Application.Commands.CreateClassroom;
using Lienzo.Application.Commands.DeleteClassroom;
using Lienzo.Application.Commands.SyncClassrooms;
using Lienzo.Application.Commands.UpdateClassroom;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.CheckAvailability;
using Lienzo.Application.Queries.GetAllClassrooms;
using Lienzo.Application.Queries.GetClassroomById;
using Lienzo.Application.Queries.GetClassroomSchedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize]
public class ClassroomsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? buildingId,
        [FromQuery] string? type,
        [FromQuery] int? minCapacity,
        [FromQuery] DateTime? date,
        [FromQuery] TimeOnly? startTime,
        [FromQuery] TimeOnly? endTime,
        [FromQuery] string? search)
    {
        var query = new GetAllClassroomsQuery(buildingId, type, minCapacity, date, startTime, endTime, search);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetClassroomByIdQuery(id));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}/availability")]
    public async Task<IActionResult> CheckAvailability(
        Guid id,
        [FromQuery] DateTime date,
        [FromQuery] TimeOnly startTime,
        [FromQuery] TimeOnly endTime)
    {
        var result = await Mediator.Send(new CheckAvailabilityQuery(id, date, startTime, endTime));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}/schedule")]
    public async Task<IActionResult> GetSchedule(Guid id, [FromQuery] int days = 7)
    {
        var result = await Mediator.Send(new GetClassroomScheduleQuery(id, days));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassroomRequest request)
    {
        var result = await Mediator.Send(new CreateClassroomCommand(request));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassroomRequest request)
    {
        var result = await Mediator.Send(new UpdateClassroomCommand(id, request));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteClassroomCommand(id));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var result = await Mediator.Send(new SyncClassroomsCommand());
        return HandleResult(result);
    }
}
