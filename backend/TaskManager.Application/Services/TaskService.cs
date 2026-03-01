using TaskManager.Application.DTOs;
using TaskManager.Application.Services.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService {
    private static readonly string[] AllowedSortFields = ["createdat", "duedate", "priority", "title", "status"];
    private static readonly string[] AllowedSortDirections = ["asc", "desc"];
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository) {
        _taskRepository = taskRepository;
    }

    public async Task<PagedResultDto<TaskResponseDto>> GetAllAsync(Guid userId, TaskListQueryDto query) {
        ValidateListQuery(query);

        var tasks = await _taskRepository.GetAllAsync(userId);
        var filteredTasks = ApplyFilters(tasks, query);
        var sortedTasks = ApplySorting(filteredTasks, query.SortBy, query.SortDirection);

        var totalItems = sortedTasks.Count();
        var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);

        var pagedItems = sortedTasks
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToResponseDto)
            .ToList();

        return new PagedResultDto<TaskResponseDto> {
            Items = pagedItems,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task<TaskResponseDto?> GetByIdAsync(Guid id, Guid userId) {
        var task = await _taskRepository.GetByIdAsync(id, userId);

        return task is null ? null : MapToResponseDto(task);
    }

    public async Task<TaskResponseDto> CreateAsync(Guid userId, CreateTaskDto createTaskDto) {
        ValidatePriority(createTaskDto.Priority);

        var title = NormalizeTitle(createTaskDto.Title);
        var description = NormalizeDescription(createTaskDto.Description);
        var dueDate = NormalizeDueDate(createTaskDto.DueDate);

        var task = new TaskItem(userId, title, description, createTaskDto.Priority, dueDate);

        await _taskRepository.AddAsync(task);

        return MapToResponseDto(task);
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, UpdateTaskDto updateTaskDto) {
        ValidatePriority(updateTaskDto.Priority);
        ValidateStatus(updateTaskDto.Status);

        var task = await _taskRepository.GetByIdAsync(id, userId);

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

    public async Task<bool> DeleteAsync(Guid id, Guid userId) {
        var task = await _taskRepository.GetByIdAsync(id, userId);

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

    private static IEnumerable<TaskItem> ApplyFilters(IEnumerable<TaskItem> tasks, TaskListQueryDto query) {
        var filteredTasks = tasks;

        if (query.Status.HasValue) {
            filteredTasks = filteredTasks.Where(task => task.Status == query.Status.Value);
        }

        if (query.Priority.HasValue) {
            filteredTasks = filteredTasks.Where(task => task.Priority == query.Priority.Value);
        }

        return filteredTasks;
    }

    private static IEnumerable<TaskItem> ApplySorting(IEnumerable<TaskItem> tasks, string sortBy, string sortDirection) {
        var normalizedSortBy = sortBy.ToLowerInvariant();
        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (normalizedSortBy, isDescending) switch {
            ("createdat", true) => tasks.OrderByDescending(task => task.CreatedAt),
            ("createdat", false) => tasks.OrderBy(task => task.CreatedAt),
            ("duedate", true) => tasks.OrderByDescending(task => task.DueDate),
            ("duedate", false) => tasks.OrderBy(task => task.DueDate),
            ("priority", true) => tasks.OrderByDescending(task => task.Priority),
            ("priority", false) => tasks.OrderBy(task => task.Priority),
            ("title", true) => tasks.OrderByDescending(task => task.Title),
            ("title", false) => tasks.OrderBy(task => task.Title),
            ("status", true) => tasks.OrderByDescending(task => task.Status),
            ("status", false) => tasks.OrderBy(task => task.Status),
            _ => tasks.OrderByDescending(task => task.CreatedAt)
        };
    }

    private static void ValidateListQuery(TaskListQueryDto query) {
        if (query.Page < 1) {
            throw new ArgumentException("Página deve ser maior ou igual à 1.");
        }

        if (query.PageSize < 1 || query.PageSize > 100) {
            throw new ArgumentException("Tamanho da página deve ser de 1 até 100.");
        }

        if (!AllowedSortFields.Contains(query.SortBy.ToLowerInvariant())) {
            throw new ArgumentException("Filtro de ordenação inválido.");
        }

        if (!AllowedSortDirections.Contains(query.SortDirection.ToLowerInvariant())) {
            throw new ArgumentException("Direção de ordenação inválida.");
        }

        if (query.Status.HasValue) {
            ValidateStatus(query.Status.Value);
        }

        if (query.Priority.HasValue) {
            ValidatePriority(query.Priority.Value);
        }
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