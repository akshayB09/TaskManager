using MediatR;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tasks.Commands;

public record AddTaskCommand(
    string Title,
    string Description,
    DateTime? DueDate,
    Priority Priority) : IRequest<TaskItem>;

public class AddTaskHandler(ITaskRepository repo)
    : IRequestHandler<AddTaskCommand, TaskItem>
{
    public Task<TaskItem> Handle(AddTaskCommand cmd, CancellationToken ct)
        => repo.AddAsync(cmd.Title, cmd.Description, cmd.DueDate, cmd.Priority);
}
