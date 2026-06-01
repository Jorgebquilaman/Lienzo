using Lienzo.Application.Commands.AdminCreateUser;
using Lienzo.Application.Commands.AdminUpdateUser;
using Lienzo.Application.Commands.ToggleUserStatus;
using Lienzo.Application.DTOs;
using Lienzo.Application.Queries.GetAllUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lienzo.API.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllUsersQuery());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminCreateUserRequest request)
    {
        var result = await Mediator.Send(new AdminCreateUserCommand(request.Email, request.Password, request.FirstName, request.LastName, request.Role));
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AdminUpdateUserRequest request)
    {
        var result = await Mediator.Send(new AdminUpdateUserCommand(id, request.Email, request.FirstName, request.LastName, request.Role));
        return HandleResult(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var result = await Mediator.Send(new ToggleUserStatusCommand(id));
        return HandleResult(result);
    }
}

public record AdminCreateUserRequest(string Email, string Password, string FirstName, string LastName, string Role);
public record AdminUpdateUserRequest(string Email, string FirstName, string LastName, string Role);
