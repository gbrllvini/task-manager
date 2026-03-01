namespace TaskManager.Application.DTOs;

public class AuthResponseDto {
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public CurrentUserDto User { get; init; } = new();
}
