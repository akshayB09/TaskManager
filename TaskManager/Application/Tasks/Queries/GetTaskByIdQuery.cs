using MediatR;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Tasks.Queries;

public record GetTaskByIdQuery(Guid Id) : IRequest<TaskItem?>;

public class GetTaskByIdHandler(ITaskRepository repo)
    : IRequestHandler<GetTaskByIdQuery, TaskItem?>
{
    public Task<TaskItem?> Handle(GetTaskByIdQuery request, CancellationToken ct)
        => repo.GetByIdAsync(request.Id);
}
