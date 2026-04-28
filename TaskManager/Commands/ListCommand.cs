using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Commands;

public class ListCommand(TaskService service) : ICommand
{
    public string Name => "list";
    public string Description => "List tasks (default: pending only)";

    public void Execute(string[] args)
    {
        var (_, named) = ArgParser.Parse(args);
        var tasks = service.GetAll().AsEnumerable();

        // --all overrides all other status filters
        if (!named.ContainsKey("all"))
            tasks = named.ContainsKey("done")
                ? tasks.Where(t => t.IsCompleted)
                : tasks.Where(t => !t.IsCompleted);

        if (named.TryGetValue("priority", out var priorityStr) &&
            Enum.TryParse<Priority>(priorityStr, ignoreCase: true, out var priority))
        {
            tasks = tasks.Where(t => t.Priority == priority);
        }

        var sorted = tasks
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
            .ToList();

        Printer.PrintTaskList(sorted);
    }
}
