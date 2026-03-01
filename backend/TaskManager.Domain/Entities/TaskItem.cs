using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Domain.Entities;

public class TaskItem {
    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public TaskPriority Priority { get; private set; }

    public TaskStatus Status { get; private set; }

    public DateTime? DueDate { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private TaskItem() { }

    public TaskItem(string title, string? description, TaskPriority priority, DateTime? dueDate) {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Priority = priority;
        Status = TaskStatus.Pending;
        DueDate = dueDate;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string? description, TaskPriority priority, DateTime? dueDate) {
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
    }

    public void UpdateStatus(TaskStatus status) {
        Status = status;
    }
}