using TaskManager.Models;

namespace TaskManager.Services;

public static class Printer
{
    public static void Success(string msg) => Colorln(msg, ConsoleColor.Green);
    public static void Error(string msg) => Colorln(msg, ConsoleColor.Red);
    public static void Info(string msg) => Colorln(msg, ConsoleColor.Cyan);
    public static void Warning(string msg) => Colorln(msg, ConsoleColor.Yellow);

    public static void PrintTaskList(IReadOnlyList<TaskItem> tasks)
    {
        if (tasks.Count == 0)
        {
            Info("No tasks found.");
            return;
        }

        Colorln($"\n  {"ID",-10} {"Title",-30} {"Due",-12} {"Priority",-10} {"Status"}", ConsoleColor.Cyan);
        Colorln("  " + new string('─', 72), ConsoleColor.DarkGray);

        foreach (var task in tasks)
            PrintTaskRow(task);

        Console.WriteLine();
    }

    public static void PrintTaskRow(TaskItem task)
    {
        var id = task.Id.ToString()[..8];
        var title = Truncate(task.Title, 28);
        var due = task.DueDate?.ToString("yyyy-MM-dd") ?? "(none)    ";
        var status = task.IsCompleted ? "Done" : "Pending";

        var color = task.IsCompleted
            ? ConsoleColor.DarkGray
            : task.Priority switch
            {
                Priority.High => ConsoleColor.Red,
                Priority.Medium => ConsoleColor.Yellow,
                Priority.Low => ConsoleColor.Green,
                _ => ConsoleColor.White
            };

        Colorln($"  {id,-10} {title,-30} {due,-12} {task.Priority,-10} {status}", color);

        if (!string.IsNullOrWhiteSpace(task.Description))
            Colorln($"  {"",10} {task.Description}", ConsoleColor.DarkGray);
    }

    private static void Colorln(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "..";
}
