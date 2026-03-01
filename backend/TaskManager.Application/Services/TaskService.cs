using TaskManager.Application.DTOs;
using TaskManager.Application.Services.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService {
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository) {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetAllAsync(TaskStatus? status = null) {
        if (status.HasValue && !Enum.IsDefined(status.Value)) {
            throw new ArgumentException("Filtro de status inválido.");
        }

        var tasks = await _taskRepository.GetAllAsync();

        if (status.HasValue) {
            tasks = tasks.Where(task => task.Status == status.Value);
        }

        return tasks.Select(MapToResponseDto);
    }

    public async Task<TaskResponseDto?> GetByIdAsync(Guid id) {
        var task = await _taskRepository.GetByIdAsync(id);

        return task is null ? null : MapToResponseDto(task);
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto createTaskDto) {
        ValidatePriority(createTaskDto.Priority);

        var title = NormalizeTitle(createTaskDto.Title);
        var description = NormalizeDescription(createTaskDto.Description);
        var dueDate = NormalizeDueDate(createTaskDto.DueDate);

        var task = new TaskItem(title, description, createTaskDto.Priority, dueDate);

        await _taskRepository.AddAsync(task);

        return MapToResponseDto(task);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTaskDto updateTaskDto) {
        ValidatePriority(updateTaskDto.Priority);
        ValidateStatus(updateTaskDto.Status);

        var task = await _taskRepository.GetByIdAsync(id);

        if (task is null) {
            return false;
        }

        var title = NormalizeTitle(updateTaskDto.Title);
        var description = NormalizeDescription(updateTaskDto.Description);
        var dueDate = NormalizeDueDate(updateTaskDto.DueDate);

        task.Update(title, description, updateTaskDto.Priority, dueDate);
        task.UpdateStatus(updateTaskDto.Status);

        await _taskRepository.UpdateAsync(task);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id) {
        var task = await _taskRepository.GetByIdAsync(id);

        if (task is null) {
            return false;
        }

        await _taskRepository.DeleteAsync(task);

        return true;
    }

    private static TaskResponseDto MapToResponseDto(TaskItem task) {
        return new TaskResponseDto {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority,
            Status = task.Status,
            DueDate = task.DueDate?.ToLocalTime(),
            CreatedAt = task.CreatedAt
        };
    }

    private static string NormalizeTitle(string title) {
        if (string.IsNullOrWhiteSpace(title)) {
            throw new ArgumentException("Título da tarefa é obrigatório.");
        }

        var normalizedTitle = title.Trim();

        if (normalizedTitle.Length < 3 || normalizedTitle.Length > 150) {
            throw new ArgumentException("Título da tarefa deve ter de 3 a 150 caracteres.");
        }

        return normalizedTitle;
    }

    private static string? NormalizeDescription(string? description) {
        if (string.IsNullOrWhiteSpace(description)) {
            return null;
        }

        var normalizedDescription = description.Trim();

        if (normalizedDescription.Length > 500) {
            throw new ArgumentException("A descrição da tarefa pode ter no máximo 500 caracteres.");
        }

        return normalizedDescription;
    }

    private static DateTime? NormalizeDueDate(DateTime? dueDate) {
        if (!dueDate.HasValue) {
            return null;
        }

        var localDueDate = DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Local);

        if (localDueDate < DateTime.Now) {
            throw new ArgumentException("Data de entrega da tarefa não pode ser retroativa.");
        }

        return localDueDate.ToUniversalTime();
    }

    private static void ValidatePriority(TaskPriority priority) {
        if (!Enum.IsDefined(priority)) {
            throw new ArgumentException("Prioridade inválida.");
        }
    }

    private static void ValidateStatus(TaskStatus status) {
        if (!Enum.IsDefined(status)) {
            throw new ArgumentException("Status inválido.");
        }
    }
}