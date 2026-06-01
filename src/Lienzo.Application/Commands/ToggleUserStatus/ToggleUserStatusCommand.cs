using Lienzo.Application.Common.Models;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.ToggleUserStatus;

public record ToggleUserStatusCommand(Guid UserId) : IRequest<Result<bool>>;

public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, Result<bool>>
{
    private readonly IAuthService _authService;

    public ToggleUserStatusCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<bool>> Handle(ToggleUserStatusCommand command, CancellationToken cancellationToken)
        => await _authService.ToggleUserStatusAsync(command.UserId);
}
