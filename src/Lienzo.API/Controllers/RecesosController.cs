using Lienzo.Application.Commands.CreateReceso;
using Lienzo.Application.Commands.DeleteReceso;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetAllRecesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class RecesosController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllRecesosQuery());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecesoRequest request)
    {
        var result = await Mediator.Send(new CreateRecesoCommand(request));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteRecesoCommand(id));
        return HandleResult(result);
    }
}
