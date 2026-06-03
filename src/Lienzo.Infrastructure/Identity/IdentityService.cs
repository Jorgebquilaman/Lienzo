using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Lienzo.Application.Commands.AdminCreateUser;
using Lienzo.Application.Commands.AdminUpdateUser;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lienzo.Infrastructure.Identity;

public class IdentityService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly JwtSettings _jwtSettings;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<AuthResponse>.Failure("Invalid email or password.", "INVALID_CREDENTIALS");

        if (!user.IsActive)
            return Result<AuthResponse>.Failure("Account is deactivated.", "ACCOUNT_DEACTIVATED");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Result<AuthResponse>.Failure("Invalid email or password.", "INVALID_CREDENTIALS");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result<AuthResponse>.Failure("Email is already registered.", "EMAIL_EXISTS");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result<AuthResponse>.Failure(errors, "REGISTRATION_FAILED");
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
            await _roleManager.CreateAsync(new IdentityRole<Guid>(request.Role));

        await _userManager.AddToRoleAsync(user, request.Role);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal is null)
            return Result<AuthResponse>.Failure("Invalid token.", "INVALID_TOKEN");

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Result<AuthResponse>.Failure("Invalid token.", "INVALID_TOKEN");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.", "INVALID_REFRESH_TOKEN");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<UserDto>.Failure("User not found.", "USER_NOT_FOUND");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        return Result<UserDto>.Success(new UserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            role,
            user.AvatarUrl,
            user.IsActive));
    }

    public async Task<Result<List<AdminUserListItemDto>>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.OrderBy(u => u.CreatedAt).ToListAsync();
        var result = new List<AdminUserListItemDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            result.Add(new AdminUserListItemDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                role,
                user.AvatarUrl,
                user.IsActive,
                user.CreatedAt));
        }

        return Result<List<AdminUserListItemDto>>.Success(result);
    }

    public async Task<Result<bool>> ToggleUserStatusAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<bool>.Failure("User not found.", "USER_NOT_FOUND");

        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Result<bool>.Failure("Failed to update user status.", "UPDATE_FAILED");

        return Result<bool>.Success(true);
    }

    public async Task<Result<AdminUserListItemDto>> AdminCreateUserAsync(AdminCreateUserCommand command)
    {
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser is not null)
            return Result<AdminUserListItemDto>.Failure("El correo ya está registrado.", "EMAIL_EXISTS");

        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result<AdminUserListItemDto>.Failure(errors, "REGISTRATION_FAILED");
        }

        if (!await _roleManager.RoleExistsAsync(command.Role))
            await _roleManager.CreateAsync(new IdentityRole<Guid>(command.Role));

        await _userManager.AddToRoleAsync(user, command.Role);

        return Result<AdminUserListItemDto>.Success(new AdminUserListItemDto(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            command.Role,
            user.AvatarUrl,
            user.IsActive,
            user.CreatedAt));
    }

    public async Task<Result<AdminUserListItemDto>> AdminUpdateUserAsync(AdminUpdateUserCommand command)
    {
        var user = await _userManager.FindByIdAsync(command.Id.ToString());
        if (user is null)
            return Result<AdminUserListItemDto>.Failure("User not found.", "USER_NOT_FOUND");

        user.Email = command.Email;
        user.UserName = command.Email;
        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            return Result<AdminUserListItemDto>.Failure(errors, "UPDATE_FAILED");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(command.Role))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!await _roleManager.RoleExistsAsync(command.Role))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(command.Role));
            await _userManager.AddToRoleAsync(user, command.Role);
        }

        return Result<AdminUserListItemDto>.Success(new AdminUserListItemDto(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            command.Role,
            user.AvatarUrl,
            user.IsActive,
            user.CreatedAt));
    }

    public async Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            return Result<bool>.Failure("Passwords do not match.", "PASSWORD_MISMATCH");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<bool>.Failure("User not found.", "USER_NOT_FOUND");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result<bool>.Failure(errors, "PASSWORD_CHANGE_FAILED");
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<string>.Success("Si el correo existe, recibirás un código de recuperación.");

        var code = Random.Shared.Next(100000, 999999).ToString();
        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _userManager.UpdateAsync(user);

        return Result<string>.Success(code);
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            return Result<bool>.Failure("Las contraseñas no coinciden.", "PASSWORD_MISMATCH");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.PasswordResetCode != request.Code || user.PasswordResetCodeExpiry < DateTime.UtcNow)
            return Result<bool>.Failure("Código inválido o expirado.", "INVALID_RESET_CODE");

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return Result<bool>.Failure("Error al restablecer la contraseña.", "RESET_FAILED");

        var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addResult.Succeeded)
        {
            var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
            return Result<bool>.Failure(errors, "RESET_FAILED");
        }

        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiry = null;
        await _userManager.UpdateAsync(user);

        return Result<bool>.Success(true);
    }

    public async Task<int?> GetSgaPersonaIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.SgaPersonaId;
    }

    public async Task<List<(Guid UserId, string Email)>> GetUsersBySgaPersonaIdsAsync(List<int> personaIds)
    {
        if (personaIds.Count == 0) return [];

        return await _userManager.Users
            .Where(u => u.SgaPersonaId.HasValue && personaIds.Contains(u.SgaPersonaId.Value))
            .Select(u => new { u.Id, u.Email })
            .ToListAsync()
            .ContinueWith(t => t.Result.Select(u => (u.Id, u.Email ?? "")).ToList());
    }

    private async Task<Result<AuthResponse>> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        var token = GenerateJwtToken(user, roles);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
        await _userManager.UpdateAsync(user);

        var userDto = new UserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            role,
            user.AvatarUrl,
            user.IsActive);

        return Result<AuthResponse>.Success(new AuthResponse(token, refreshToken, expiresAt, userDto));
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Secret)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
