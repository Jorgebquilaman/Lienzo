using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.ResetPassword;

public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<Result<bool>>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<Result<bool>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        return await _authService.ResetPasswordAsync(command.Request);
    }
}
