using Application.DTOs.Auth;
using Domain.Models;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<(AuthResponseDto Response, string? RefreshToken)> RegisterAsync(RegisterRequestDto registerDto);
    Task<(AuthResponseDto Response, string? RefreshToken)> LoginAsync(LoginRequestDto loginDto);
    Task<(AuthResponseDto Response, string? RefreshToken)> RefreshTokenAsync(string refreshToken);
    Task<ChangePasswordResponseDto> ChangePasswordAsync(string userId, ChangePasswordRequestDto changePasswordDto);
    Task<UserProfileResponseDto?> GetUserProfileAsync(string userId);
    Task RevokeTokenAsync(string refreshToken, string reason = "Revoked by user");
    Task RevokeAllUserTokensAsync(string userId, string reason = "Revoked all tokens");
}
