using TaskManager.Application.DTOs;

namespace TaskManager.Application.Services.Interfaces;

public interface ITaskService {
    Task<PagedResultDto<TaskResponseDto>> GetAllAsync(Guid userId, TaskListQueryDto query);
    Task<TaskResponseDto?> GetByIdAsync(Guid id, Guid userId);
    Task<TaskResponseDto> CreateAsync(Guid userId, CreateTaskDto createTaskDto);
    Task<bool> UpdateAsync(Guid id, Guid userId, UpdateTaskDto updateTaskDto);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
