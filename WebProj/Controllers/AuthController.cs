using System.Security.Claims;
using Application.DTOs.Auth;
using Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebProj.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<ChangePasswordRequestDto> _changePasswordValidator;
    private readonly IWebHostEnvironment _environment;
    private const string RefreshTokenCookieName = "refreshToken";

    public AuthController(
        IAuthService authService,
        IJwtService jwtService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<ChangePasswordRequestDto> changePasswordValidator,
        IWebHostEnvironment environment)
    {
        _authService = authService;
        _jwtService = jwtService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
        _environment = environment;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
    {
        var validationResult = await _registerValidator.ValidateAsync(registerDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var (response, refreshToken) = await _authService.RegisterAsync(registerDto);

        if (!response.Success)
        {
            return BadRequest(response);
        }
        if (!string.IsNullOrEmpty(refreshToken))
        {
            SetRefreshTokenCookie(refreshToken);
        }

        return Ok(response);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        var validationResult = await _loginValidator.ValidateAsync(loginDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var (response, refreshToken) = await _authService.LoginAsync(loginDto);

        if (!response.Success)
        {
            return Unauthorized(response);
        }
        if (!string.IsNullOrEmpty(refreshToken))
        {
            SetRefreshTokenCookie(refreshToken);
        }

        return Ok(response);
    }

    /// <summary>
    /// Refresh access token using refresh token from cookie
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Errors = new[] { "Refresh token not found" }
            });
        }

        var (response, newRefreshToken) = await _authService.RefreshTokenAsync(refreshToken);

        if (!response.Success)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized(response);
        }
        if (!string.IsNullOrEmpty(newRefreshToken))
        {
            SetRefreshTokenCookie(newRefreshToken);
        }

        return Ok(response);
    }

    /// <summary>
    /// Change password (requires authentication and current password)
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordDto)
    {
        var validationResult = await _changePasswordValidator.ValidateAsync(changePasswordDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ChangePasswordResponseDto
            {
                Success = false,
                Errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        // Clear refresh token cookie after password change
        DeleteRefreshTokenCookie();

        return Ok(result);
    }

    /// <summary>
    /// Logout - revokes the current refresh token
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.RevokeTokenAsync(refreshToken, "User logged out");
        }

        DeleteRefreshTokenCookie();

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var profile = await _authService.GetUserProfileAsync(userId);

        if (profile == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(profile);
    }

    /// <summary>
    /// Revoke all refresh tokens for the current user (logout from all devices)
    /// </summary>
    [HttpPost("revoke-all-tokens")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeAllTokens()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _authService.RevokeAllUserTokensAsync(userId, "User revoked all tokens");
        DeleteRefreshTokenCookie();

        return Ok(new { message = "All tokens revoked successfully" });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(), 
            SameSite = SameSiteMode.Lax, // Lax allows the cookie to be sent on navigation
            Expires = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpirationDays()),
            Path = "/", 
            IsEssential = true
        };

        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    }

    private void DeleteRefreshTokenCookie()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = "/" 
        };

        Response.Cookies.Delete(RefreshTokenCookieName, cookieOptions);
    }
}
