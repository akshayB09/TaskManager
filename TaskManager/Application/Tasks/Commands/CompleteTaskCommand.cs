using MediatR;
using TaskManager.Application.Common;

namespace TaskManager.Application.Tasks.Commands;

public record CompleteTaskCommand(Guid Id) : IRequest<bool>;

public class CompleteTaskHandler(ITaskRepository repo)
    : IRequestHandler<CompleteTaskCommand, bool>
{
    public Task<bool> Handle(CompleteTaskCommand cmd, CancellationToken ct)
        => repo.CompleteAsync(cmd.Id);
}
