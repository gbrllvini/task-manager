using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.DTOs;

public class TaskListQueryDto {
    public TaskStatus? Status { get; init; }
    public TaskPriority? Priority { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortBy { get; init; } = "createdAt";
    public string SortDirection { get; init; } = "desc";
}