using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.ChangePassword;

public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Request) : IRequest<Result<bool>>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly IAuthService _authService;

    public ChangePasswordCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<Result<bool>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        => await _authService.ChangePasswordAsync(command.UserId, command.Request);
}
