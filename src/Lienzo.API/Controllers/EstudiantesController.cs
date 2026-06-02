using Lienzo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class EstudiantesController : BaseApiController
{
    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] SyncEstudiantesRequest request, [FromServices] ISyncEstudianteService syncService)
    {
        var result = await syncService.CreateMissingEstudianteUsersAsync(request.AnioAcademico);
        return Ok(result);
    }
}

public record SyncEstudiantesRequest(short AnioAcademico);
