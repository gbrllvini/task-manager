namespace TaskManager.Application.Auth;

public class RefreshTokenIssueResult {
    public string RawToken { get; init; } = string.Empty;
    public string TokenHash { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
