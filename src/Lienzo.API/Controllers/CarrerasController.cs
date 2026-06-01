using Lienzo.Application.Commands.CreateCarrera;
using Lienzo.Application.Commands.DeleteCarrera;
using Lienzo.Application.Commands.SyncCarreras;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetAllCarreras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class CarrerasController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllCarrerasQuery());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCarreraRequest request)
    {
        var result = await Mediator.Send(new CreateCarreraCommand(request));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteCarreraCommand(id));
        return HandleResult(result);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var result = await Mediator.Send(new SyncCarrerasCommand());
        return HandleResult(result);
    }
}
