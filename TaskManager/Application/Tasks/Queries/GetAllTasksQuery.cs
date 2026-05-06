using MediatR;
using TaskManager.Application.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Tasks.Queries;

public record GetAllTasksQuery : IRequest<IReadOnlyList<TaskItem>>;

public class GetAllTasksHandler(ITaskRepository repo)
    : IRequestHandler<GetAllTasksQuery, IReadOnlyList<TaskItem>>
{
    public Task<IReadOnlyList<TaskItem>> Handle(GetAllTasksQuery request, CancellationToken ct)
        => repo.GetAllAsync();
}
