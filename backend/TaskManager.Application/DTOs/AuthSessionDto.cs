namespace TaskManager.Application.DTOs;

public class AuthSessionDto {
    public AuthResponseDto Response { get; init; } = new();
    public string RefreshToken { get; init; } = string.Empty;
}
