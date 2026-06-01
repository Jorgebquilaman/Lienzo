using Lienzo.Application.Commands.CreateActividad;
using Lienzo.Application.Commands.DeleteActividad;
using Lienzo.Application.Commands.SyncActividades;
using Lienzo.Application.Commands.UpdateActividad;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Application.Queries.GetAllActividades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize]
public class ActividadesController : BaseApiController
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllActividadesQuery());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActividadRequest request)
    {
        var result = await Mediator.Send(new CreateActividadCommand(request));
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateActividadRequest request)
    {
        var result = await Mediator.Send(new UpdateActividadCommand(id, request));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteActividadCommand(id));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromQuery] short anioAcademico = 2026)
    {
        var result = await Mediator.Send(new SyncActividadesCommand(anioAcademico));
        return HandleResult(result);
    }

    [HttpPost("sync-docentes")]
    public async Task<IActionResult> SyncDocentes([FromBody] SyncDocentesRequest request, [FromServices] ISyncDocenteService syncDocenteService)
    {
        var result = await syncDocenteService.CreateMissingDocenteUsersAsync(request.AnioAcademico);
        return Ok(result);
    }
}

public record SyncDocentesRequest(short AnioAcademico);
