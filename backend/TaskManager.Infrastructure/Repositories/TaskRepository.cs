using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository {
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context) {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(Guid userId) {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId) {
        return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task AddAsync(TaskItem task) {
        await _context.Tasks.AddAsync(task);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem task) {
        _context.Tasks.Update(task);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task) {
        _context.Tasks.Remove(task);
        
        await _context.SaveChangesAsync();
    }
}
