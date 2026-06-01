using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.ForgotPassword;

public record ForgotPasswordCommand(ForgotPasswordRequest Request) : IRequest<Result<string>>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly IAuthService _authService;

    public ForgotPasswordCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<Result<string>> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        return await _authService.ForgotPasswordAsync(command.Request);
    }
}
