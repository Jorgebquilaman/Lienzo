using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<List<AdminUserListItemDto>>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<AdminUserListItemDto>>>
{
    private readonly IAuthService _authService;

    public GetAllUsersQueryHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<List<AdminUserListItemDto>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
        => await _authService.GetAllUsersAsync();
}
