using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Commands;

public class EditCommand(TaskService service) : ICommand
{
    public string Name => "edit";
    public string Description => "Edit a task's fields";

    public void Execute(string[] args)
    {
        var (positional, named) = ArgParser.Parse(args);
        if (positional.Count == 0)
        {
            Printer.Error("Task ID required.");
            Printer.Info("Usage: tm edit <id> [--title \"...\"] [--description \"...\"] [--due YYYY-MM-DD] [--priority Low|Medium|High]");
            return;
        }

        var (task, error) = service.FindByPrefix(positional[0]);
        if (error is not null) { Printer.Error(error); return; }

        named.TryGetValue("title", out var title);
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

        Priority? priority = null;
        if (named.TryGetValue("priority", out var priorityStr))
        {
            if (!Enum.TryParse<Priority>(priorityStr, ignoreCase: true, out var p))
            {
                Printer.Error($"Invalid priority '{priorityStr}'. Use Low, Medium, or High.");
                return;
            }
            priority = p;
        }

        if (title is null && description is null && dueDate is null && priority is null)
        {
            Printer.Warning("Nothing to update — pass at least one field to change.");
            return;
        }

        service.Edit(task!.Id, title, description, dueDate, priority);
        Printer.Success($"Updated '{task.Title}'.");
        Printer.PrintTaskRow(service.GetById(task.Id)!);
    }
}
