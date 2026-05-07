using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Tasks.Commands;
using TaskManager.Application.Tasks.Queries;
using TaskManager.Domain.Enums;

namespace TaskManager.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public class TasksController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await mediator.Send(new GetAllTasksQuery()));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest req)
    {
        var task = await mediator.Send(new AddTaskCommand(req.Title, req.Description ?? "", req.DueDate, req.Priority));
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var task = await mediator.Send(new GetTaskByIdQuery(id));
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
        => await mediator.Send(new CompleteTaskCommand(id)) ? Ok() : NotFound();

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, EditTaskRequest req)
    {
        if (!await mediator.Send(new EditTaskCommand(id, req.Title, req.Description, req.DueDate, req.Priority)))
            return NotFound();
        return Ok(await mediator.Send(new GetTaskByIdQuery(id)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => await mediator.Send(new DeleteTaskCommand(id)) ? Ok() : NotFound();
}

public record CreateTaskRequest(string Title, string? Description, DateTime? DueDate, Priority Priority);
public record EditTaskRequest(string? Title, string? Description, DateTime? DueDate, Priority? Priority);
