using Lienzo.Application.Commands.CreatePeriodo;
using Lienzo.Application.Commands.DeletePeriodo;
using Lienzo.Application.Commands.SyncPeriodos;
using Lienzo.Application.Commands.SyncTiposPeriodo;
using Lienzo.Application.Commands.TogglePeriodoActive;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetAllPeriodos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class PeriodosController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllPeriodosQuery());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePeriodoRequest request)
    {
        var result = await Mediator.Send(new CreatePeriodoCommand(request));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeletePeriodoCommand(id));
        return HandleResult(result);
    }

    [HttpPost("sync-tipos")]
    public async Task<IActionResult> SyncTipos()
    {
        var result = await Mediator.Send(new SyncTiposPeriodoCommand());
        return HandleResult(result);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromQuery] short anioAcademico = 2026)
    {
        var result = await Mediator.Send(new SyncPeriodosCommand(anioAcademico));
        return HandleResult(result);
    }

    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var result = await Mediator.Send(new TogglePeriodoActiveCommand(id));
        return HandleResult(result);
    }
}
