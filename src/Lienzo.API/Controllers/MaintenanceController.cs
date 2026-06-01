using Lienzo.Application.Commands.CreateMaintenanceBlock;
using Lienzo.Application.Commands.DeleteMaintenanceBlock;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetMaintenanceBlocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class MaintenanceController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, [FromQuery] Guid? classroomId)
    {
        var result = await Mediator.Send(new GetMaintenanceBlocksQuery(activeOnly, classroomId));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceBlockRequest request)
    {
        var result = await Mediator.Send(new CreateMaintenanceBlockCommand(request));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteMaintenanceBlockCommand(id));
        return HandleResult(result);
    }
}
