using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository {
    Task<IEnumerable<TaskItem>> GetAllAsync(Guid userId);
    Task<TaskItem?> GetByIdAsync(Guid id, Guid userId);
    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
}