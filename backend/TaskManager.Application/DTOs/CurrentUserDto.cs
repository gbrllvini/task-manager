namespace TaskManager.Application.DTOs;

public class CurrentUserDto {
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
