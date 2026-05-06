using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Common;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<TaskItem> AddAsync(string title, string description, DateTime? dueDate, Priority priority);
    Task<bool> CompleteAsync(Guid id);
    Task<bool> EditAsync(Guid id, string? title, string? description, DateTime? dueDate, Priority? priority);
    Task<bool> DeleteAsync(Guid id);
}
