using System.Text.Json;
using TaskManager.Models;

namespace TaskManager.Services;

public class TaskService
{
    private readonly string _filePath;
    private List<TaskItem> _tasks;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public TaskService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".taskmanager");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "tasks.json");
        _tasks = Load();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public IReadOnlyList<TaskItem> GetAll() => _tasks.AsReadOnly();

    public TaskItem? GetById(Guid id) => _tasks.FirstOrDefault(t => t.Id == id);

    /// Finds a task by a short ID prefix. Returns null and prints an error if
    /// the prefix matches zero or more than one task.
    public (TaskItem? Task, string? Error) FindByPrefix(string prefix)
    {
        var matches = _tasks
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
        _tasks.Add(task);
        Save();
        return task;
    }

    public bool Complete(Guid id)
    {
        var task = GetById(id);
        if (task is null) return false;
        task.IsCompleted = true;
        Save();
        return true;
    }

    public bool Delete(Guid id)
    {
        var task = GetById(id);
        if (task is null) return false;
        _tasks.Remove(task);
        Save();
        return true;
    }

    public bool Edit(Guid id, string? title, string? description, DateTime? dueDate, Priority? priority)
    {
        var task = GetById(id);
        if (task is null) return false;

        if (title is not null) task.Title = title;
        if (description is not null) task.Description = description;
        if (dueDate is not null) task.DueDate = dueDate;
        if (priority is not null) task.Priority = priority.Value;

        Save();
        return true;
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    private List<TaskItem> Load()
    {
        if (!File.Exists(_filePath)) return [];
        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<TaskItem>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            // Corrupted file — start fresh rather than crashing
            return [];
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_tasks, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
