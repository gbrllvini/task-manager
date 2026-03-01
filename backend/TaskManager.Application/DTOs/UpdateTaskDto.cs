using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.DTOs;

public class UpdateTaskDto {
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [EnumDataType(typeof(TaskPriority))]
    public TaskPriority Priority { get; set; }

    [EnumDataType(typeof(TaskStatus))]
    public TaskStatus Status { get; set; }

    public DateTime? DueDate { get; set; }
}