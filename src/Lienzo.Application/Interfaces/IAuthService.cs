using Lienzo.Application.Commands.AdminCreateUser;
using Lienzo.Application.Commands.AdminUpdateUser;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;

namespace Lienzo.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<Result<UserDto>> GetCurrentUserAsync(Guid userId);
    Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<Result<List<AdminUserListItemDto>>> GetAllUsersAsync();
    Task<Result<bool>> ToggleUserStatusAsync(Guid userId);
    Task<Result<AdminUserListItemDto>> AdminCreateUserAsync(AdminCreateUserCommand command);
    Task<Result<AdminUserListItemDto>> AdminUpdateUserAsync(AdminUpdateUserCommand command);
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<int?> GetSgaPersonaIdAsync(Guid userId);
    Task<List<(Guid UserId, string Email)>> GetUsersBySgaPersonaIdsAsync(List<int> personaIds);
}
