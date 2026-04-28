using TaskManager.Services;

namespace TaskManager.Commands;

public class CompleteCommand(TaskService service) : ICommand
{
    public string Name => "complete";
    public string Description => "Mark a task as complete";

    public void Execute(string[] args)
    {
        var (positional, _) = ArgParser.Parse(args);
        if (positional.Count == 0)
        {
            Printer.Error("Task ID required.");
            Printer.Info("Usage: tm complete <id>");
            return;
        }

        var (task, error) = service.FindByPrefix(positional[0]);
        if (error is not null) { Printer.Error(error); return; }

        if (task!.IsCompleted)
        {
            Printer.Warning($"'{task.Title}' is already complete.");
            return;
        }

        service.Complete(task.Id);
        Printer.Success($"Marked '{task.Title}' as complete.");
    }
}
