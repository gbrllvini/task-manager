using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Services.Interfaces;

public interface ITaskService {
    Task<IEnumerable<TaskResponseDto>> GetAllAsync(TaskStatus? status = null);
    Task<TaskResponseDto?> GetByIdAsync(Guid id);
    Task<TaskResponseDto> CreateAsync(CreateTaskDto createTaskDto);
    Task<bool> UpdateAsync(Guid id, UpdateTaskDto updateTaskDto);
    Task<bool> DeleteAsync(Guid id);
}