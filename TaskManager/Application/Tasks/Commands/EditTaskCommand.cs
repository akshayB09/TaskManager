using MediatR;
using TaskManager.Application.Common;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tasks.Commands;

public record EditTaskCommand(
    Guid Id,
    string? Title,
    string? Description,
    DateTime? DueDate,
    Priority? Priority) : IRequest<bool>;

public class EditTaskHandler(ITaskRepository repo)
    : IRequestHandler<EditTaskCommand, bool>
{
    public Task<bool> Handle(EditTaskCommand cmd, CancellationToken ct)
        => repo.EditAsync(cmd.Id, cmd.Title, cmd.Description, cmd.DueDate, cmd.Priority);
}
