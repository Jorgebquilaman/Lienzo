using MediatR;
using Microsoft.AspNetCore.Mvc;
using Lienzo.Application.Common.Models;

namespace Lienzo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess) return Ok(result.Value);
        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new ProblemDetails { Title = result.Error, Status = 404 }),
            "VALIDATION" => UnprocessableEntity(new ProblemDetails { Title = result.Error, Status = 422 }),
            "FORBIDDEN" => Forbid(),
            "CONFLICT" => Conflict(new ProblemDetails { Title = result.Error, Status = 409 }),
            _ => BadRequest(new ProblemDetails { Title = result.Error, Status = 400 })
        };
    }
}
