using Lienzo.Application.Commands.CreateAccessory;
using Lienzo.Application.Commands.DeleteAccessory;
using Lienzo.Application.Commands.UpdateAccessory;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetAllAccessories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class AccessoriesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllAccessoriesQuery());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccessoryRequest request)
    {
        var result = await Mediator.Send(new CreateAccessoryCommand(request.Name, request.Description));
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccessoryRequest request)
    {
        var result = await Mediator.Send(new UpdateAccessoryCommand(id, request.Name, request.Description, request.IsActive));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteAccessoryCommand(id));
        return HandleResult(result);
    }
}
