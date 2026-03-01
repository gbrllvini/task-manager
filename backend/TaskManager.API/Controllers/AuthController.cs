using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase {
    private const string RefreshCookieName = "tm_refresh_token";
    private readonly IAuthService _authService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthController(IAuthService authService, IRefreshTokenService refreshTokenService) {
        _authService = authService;
        _refreshTokenService = refreshTokenService;
    }

    [HttpPost("cadastro")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RegisterAsync([FromBody] RegisterRequestDto request, CancellationToken cancellationToken) {
        var session = await _authService.RegisterAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);

        SetRefreshCookie(session.RefreshToken);

        return Ok(session.Response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> LoginAsync([FromBody] LoginRequestDto request, CancellationToken cancellationToken) {
        var session = await _authService.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);

        SetRefreshCookie(session.RefreshToken);

        return Ok(session.Response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RefreshAsync(CancellationToken cancellationToken) {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken)) {
            throw new UnauthorizedAccessException("Refresh token ausente.");
        }

        var session = await _authService.RefreshAsync(refreshToken, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);

        SetRefreshCookie(session.RefreshToken);

        return Ok(session.Response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken) {
        var userId = GetUserId();

        Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken);

        await _authService.LogoutAsync(userId, refreshToken, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);

        RemoveRefreshCookie();

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> MeAsync(CancellationToken cancellationToken) {
        var userId = GetUserId();

        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }

    private Guid GetUserId() {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(claimValue, out var userId)) {
            throw new UnauthorizedAccessException("Token invalido.");
        }

        return userId;
    }

    private void SetRefreshCookie(string refreshToken) {
        var days = _refreshTokenService.GetRefreshTokenDays();
        
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(days)
        });
    }

    private void RemoveRefreshCookie() {
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Path = "/api/auth"
        });
    }
}
