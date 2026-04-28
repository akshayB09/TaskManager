using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Commands;

public class AddCommand(TaskService service) : ICommand
{
    public string Name => "add";
    public string Description => "Add a new task";

    public void Execute(string[] args)
    {
        var (_, named) = ArgParser.Parse(args);

        if (!named.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title))
        {
            Printer.Error("--title is required.");
            Printer.Info("Usage: tm add --title \"...\" [--description \"...\"] [--due YYYY-MM-DD] [--priority Low|Medium|High]");
            return;
        }

        named.TryGetValue("description", out var description);

        DateTime? dueDate = null;
        if (named.TryGetValue("due", out var dueStr))
        {
            if (!DateTime.TryParse(dueStr, out var parsed))
            {
                Printer.Error($"Invalid date '{dueStr}'. Use YYYY-MM-DD.");
                return;
            }
            dueDate = parsed;
        }

        var priority = Priority.Medium;
        if (named.TryGetValue("priority", out var priorityStr) &&
            !Enum.TryParse(priorityStr, ignoreCase: true, out priority))
        {
            Printer.Error($"Invalid priority '{priorityStr}'. Use Low, Medium, or High.");
            return;
        }

        var task = service.Add(title, description ?? string.Empty, dueDate, priority);
        Printer.Success($"Task added (ID: {task.Id.ToString()[..8]})");
        Printer.PrintTaskRow(task);
    }
}
