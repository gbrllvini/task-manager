namespace TaskManager.Application.Auth;

public class AccessTokenResult {
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
