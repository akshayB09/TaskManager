using MediatR;
using TaskManager.Application.Common;

namespace TaskManager.Application.Tasks.Commands;

public record DeleteTaskCommand(Guid Id) : IRequest<bool>;

public class DeleteTaskHandler(ITaskRepository repo)
    : IRequestHandler<DeleteTaskCommand, bool>
{
    public Task<bool> Handle(DeleteTaskCommand cmd, CancellationToken ct)
        => repo.DeleteAsync(cmd.Id);
}
