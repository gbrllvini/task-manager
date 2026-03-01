using TaskManager.Application.Auth;

namespace TaskManager.Application.Services.Interfaces;

public interface IRefreshTokenService {
    Task<RefreshTokenIssueResult> IssueAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default);
    Task<(Guid UserId, RefreshTokenIssueResult IssuedToken)> RotateAsync(string rawToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task RevokeAsync(Guid userId, string rawToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task RevokeCurrentAsync(string rawToken, string? ipAddress, CancellationToken cancellationToken = default);
    int GetRefreshTokenDays();
}
