using TaskManager.Application.DTOs;

namespace TaskManager.Application.Services.Interfaces;

public interface ITaskService {
    Task<PagedResultDto<TaskResponseDto>> GetAllAsync(TaskListQueryDto query);
    Task<TaskResponseDto?> GetByIdAsync(Guid id);
    Task<TaskResponseDto> CreateAsync(CreateTaskDto createTaskDto);
    Task<bool> UpdateAsync(Guid id, UpdateTaskDto updateTaskDto);
    Task<bool> DeleteAsync(Guid id);
}