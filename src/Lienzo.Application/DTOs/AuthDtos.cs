namespace Lienzo.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string Role);

public record AuthResponse(string Token, string RefreshToken, DateTime ExpiresAt, UserDto User);

public record RefreshTokenRequest(string Token, string RefreshToken);

public record UserDto(Guid Id, string Email, string FirstName, string LastName, string Role, string? AvatarUrl, bool IsActive);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmNewPassword);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Code, string NewPassword, string ConfirmNewPassword);
