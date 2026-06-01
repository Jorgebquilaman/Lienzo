using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.AdminCreateUser;

public record AdminCreateUserCommand(string Email, string Password, string FirstName, string LastName, string Role) : IRequest<Result<AdminUserListItemDto>>;

public class AdminCreateUserCommandHandler : IRequestHandler<AdminCreateUserCommand, Result<AdminUserListItemDto>>
{
    private readonly IAuthService _authService;

    public AdminCreateUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AdminUserListItemDto>> Handle(AdminCreateUserCommand command, CancellationToken cancellationToken)
        => await _authService.AdminCreateUserAsync(command);
}
