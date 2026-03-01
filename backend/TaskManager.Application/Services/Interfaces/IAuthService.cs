using TaskManager.Application.DTOs;

namespace TaskManager.Application.Services.Interfaces;

public interface IAuthService {
    Task<AuthSessionDto> RegisterAsync(RegisterRequestDto request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<AuthSessionDto> LoginAsync(LoginRequestDto request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<AuthSessionDto> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, string? refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
