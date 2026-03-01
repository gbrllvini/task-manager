using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.DTOs;

public class TaskResponseDto {
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskPriority Priority { get; init; }
    public TaskStatus Status { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime CreatedAt { get; init; }
}