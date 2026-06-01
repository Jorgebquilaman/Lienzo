using Lienzo.Application.Commands.CreateBuilding;
using Lienzo.Application.Commands.DeleteBuilding;
using Lienzo.Application.Commands.SyncBuildings;
using Lienzo.Application.Commands.UpdateBuilding;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetAllBuildings;
using Lienzo.Application.Queries.GetBuildingById;
using Lienzo.Application.Queries.GetBuildingClassrooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize]
public class BuildingsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllBuildingsQuery());
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetBuildingByIdQuery(id));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}/classrooms")]
    public async Task<IActionResult> GetClassrooms(Guid id)
    {
        var result = await Mediator.Send(new GetBuildingClassroomsQuery(id));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBuildingRequest request)
    {
        var result = await Mediator.Send(new CreateBuildingCommand(request));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBuildingRequest request)
    {
        var result = await Mediator.Send(new UpdateBuildingCommand(id, request));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteBuildingCommand(id));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var result = await Mediator.Send(new SyncBuildingsCommand());
        return HandleResult(result);
    }
}
