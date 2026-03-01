using Microsoft.AspNetCore.Identity;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services.Interfaces;
using TaskManager.Infrastructure.Identity;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService) {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthSessionDto> RegisterAsync(RegisterRequestDto request, string? ipAddress, CancellationToken cancellationToken = default) {
        var displayName = NormalizeDisplayName(request.DisplayName);
        var email = NormalizeEmail(request.Email);
        var password = request.Password.Trim();

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null) {
            throw new ArgumentException("Email já está em uso.");
        }

        var user = new ApplicationUser {
            Id = Guid.NewGuid(),
            DisplayName = displayName,
            Email = email,
            UserName = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, password);

        if (!createResult.Succeeded) {
            var message = string.Join(" ", createResult.Errors.Select(error => error.Description));

            throw new ArgumentException(message);
        }

        return await BuildSessionAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthSessionDto> LoginAsync(LoginRequestDto request, string? ipAddress, CancellationToken cancellationToken = default) {
        var email = NormalizeEmail(request.Email);
        var password = request.Password.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null) {
            throw new UnauthorizedAccessException("Email ou senha inválidos.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, false);

        if (!signInResult.Succeeded) {
            throw new UnauthorizedAccessException("Email ou senha inválidos.");
        }

        return await BuildSessionAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthSessionDto> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default) {
        if (string.IsNullOrWhiteSpace(refreshToken)) {
            throw new UnauthorizedAccessException("Sessão expirada.");
        }

        var (userId, issuedToken) = await _refreshTokenService.RotateAsync(refreshToken.Trim(), ipAddress, cancellationToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null) {
            throw new UnauthorizedAccessException("Usuário não encontrado.");
        }

        var accessToken = _tokenService.CreateAccessToken(user);

        return new AuthSessionDto {
            Response = new AuthResponseDto {
                AccessToken = accessToken.Token,
                ExpiresAtUtc = accessToken.ExpiresAtUtc,
                User = MapCurrentUser(user)
            },
            RefreshToken = issuedToken.RawToken
        };
    }

    public async Task LogoutAsync(Guid userId, string? refreshToken, string? ipAddress, CancellationToken cancellationToken = default) {
        if (string.IsNullOrWhiteSpace(refreshToken)) {
            return;
        }

        await _refreshTokenService.RevokeAsync(userId, refreshToken.Trim(), ipAddress, cancellationToken);
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default) {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user is null ? null : MapCurrentUser(user);
    }

    private async Task<AuthSessionDto> BuildSessionAsync(ApplicationUser user, string? ipAddress, CancellationToken cancellationToken) {
        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = await _refreshTokenService.IssueAsync(user.Id, ipAddress, cancellationToken);

        return new AuthSessionDto {
            Response = new AuthResponseDto {
                AccessToken = accessToken.Token,
                ExpiresAtUtc = accessToken.ExpiresAtUtc,
                User = MapCurrentUser(user)
            },
            RefreshToken = refreshToken.RawToken
        };
    }

    private static string NormalizeDisplayName(string displayName) {
        if (string.IsNullOrWhiteSpace(displayName)) {
            throw new ArgumentException("Nome é obrigatório.");
        }

        var normalized = displayName.Trim();

        if (normalized.Length < 3 || normalized.Length > 120) {
            throw new ArgumentException("Nome deve ter entre 3 e 120 caracteres.");
        }

        return normalized;
    }

    private static string NormalizeEmail(string email) {
        if (string.IsNullOrWhiteSpace(email)) {
            throw new ArgumentException("Email é obrigatório.");
        }

        return email.Trim().ToLowerInvariant();
    }

    private static CurrentUserDto MapCurrentUser(ApplicationUser user) {
        return new CurrentUserDto {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty
        };
    }
}
