using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class TaskService(TaskDbContext context)
{
    public IReadOnlyList<TaskItem> GetAll() => context.Tasks.AsNoTracking().ToList();

    public TaskItem? GetById(Guid id) => context.Tasks.AsNoTracking().FirstOrDefault(t => t.Id == id);

    public (TaskItem? Task, string? Error) FindByPrefix(string prefix)
    {
        var matches = context.Tasks
            .AsNoTracking()
            .AsEnumerable()
            .Where(t => t.Id.ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            0 => (null, $"No task found with ID starting with '{prefix}'."),
            1 => (matches[0], null),
            _ => (null, $"Ambiguous ID '{prefix}' matches {matches.Count} tasks — use more characters.")
        };
    }

    public TaskItem Add(string title, string description, DateTime? dueDate, Priority priority)
    {
        var task = new TaskItem
        {
            Title = title,
            Description = description,
            DueDate = dueDate,
            Priority = priority
        };
        context.Tasks.Add(task);
        context.SaveChanges();
        return task;
    }

    public bool Complete(Guid id)
    {
        context.ChangeTracker.Clear();
        var task = context.Tasks.FirstOrDefault(t => t.Id == id);
        if (task is null) return false;
        task.IsCompleted = true;
        context.SaveChanges();
        return true;
    }

    public bool Delete(Guid id)
    {
        context.ChangeTracker.Clear();
        var task = context.Tasks.FirstOrDefault(t => t.Id == id);
        if (task is null) return false;
        context.Tasks.Remove(task);
        context.SaveChanges();
        return true;
    }

    public bool Edit(Guid id, string? title, string? description, DateTime? dueDate, Priority? priority)
    {
        context.ChangeTracker.Clear();
        var task = context.Tasks.FirstOrDefault(t => t.Id == id);
        if (task is null) return false;

        if (title is not null) task.Title = title;
        if (description is not null) task.Description = description;
        if (dueDate is not null) task.DueDate = dueDate;
        if (priority is not null) task.Priority = priority.Value;

        context.SaveChanges();
        return true;
    }
}
