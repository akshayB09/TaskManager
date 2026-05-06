using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Persistence;

public class TaskRepository(TaskDbContext context) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync()
        => await context.Tasks.AsNoTracking().ToListAsync();

    public async Task<TaskItem?> GetByIdAsync(Guid id)
        => await context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TaskItem> AddAsync(string title, string description, DateTime? dueDate, Priority priority)
    {
        var task = new TaskItem
        {
            Title = title,
            Description = description,
            DueDate = dueDate,
            Priority = priority
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        return task;
    }

    public async Task<bool> CompleteAsync(Guid id)
    {
        context.ChangeTracker.Clear();
        var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return false;
        task.IsCompleted = true;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EditAsync(Guid id, string? title, string? description, DateTime? dueDate, Priority? priority)
    {
        context.ChangeTracker.Clear();
        var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return false;
        if (title is not null) task.Title = title;
        if (description is not null) task.Description = description;
        if (dueDate is not null) task.DueDate = dueDate;
        if (priority is not null) task.Priority = priority.Value;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        context.ChangeTracker.Clear();
        var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return false;
        context.Tasks.Remove(task);
        await context.SaveChangesAsync();
        return true;
    }
}
