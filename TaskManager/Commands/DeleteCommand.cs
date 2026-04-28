using TaskManager.Services;

namespace TaskManager.Commands;

public class DeleteCommand(TaskService service) : ICommand
{
    public string Name => "delete";
    public string Description => "Delete a task";

    public void Execute(string[] args)
    {
        var (positional, _) = ArgParser.Parse(args);
        if (positional.Count == 0)
        {
            Printer.Error("Task ID required.");
            Printer.Info("Usage: tm delete <id>");
            return;
        }

        var (task, error) = service.FindByPrefix(positional[0]);
        if (error is not null) { Printer.Error(error); return; }

        service.Delete(task!.Id);
        Printer.Success($"Deleted '{task.Title}'.");
    }
}
