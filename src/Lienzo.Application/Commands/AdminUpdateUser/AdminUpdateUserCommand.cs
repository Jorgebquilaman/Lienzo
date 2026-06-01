using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.AdminUpdateUser;

public record AdminUpdateUserCommand(Guid Id, string Email, string FirstName, string LastName, string Role) : IRequest<Result<AdminUserListItemDto>>;

public class AdminUpdateUserCommandHandler : IRequestHandler<AdminUpdateUserCommand, Result<AdminUserListItemDto>>
{
    private readonly IAuthService _authService;

    public AdminUpdateUserCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AdminUserListItemDto>> Handle(AdminUpdateUserCommand command, CancellationToken cancellationToken)
        => await _authService.AdminUpdateUserAsync(command);
}
