using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Text.Json.Serialization;
using TaskManager.Application.Common;
using TaskManager.Application.Tasks.Commands;
using TaskManager.Application.Tasks.Queries;
using TaskManager.Components;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".taskmanager", "tasks.db");

builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration);
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();
builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    db.Database.Migrate();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var api = app.MapGroup("/api/tasks").RequireAuthorization();

api.MapGet("/", (IMediator mediator) => mediator.Send(new GetAllTasksQuery()));

api.MapPost("/", async (IMediator mediator, CreateTaskRequest req) =>
{
    var task = await mediator.Send(new AddTaskCommand(req.Title, req.Description ?? "", req.DueDate, req.Priority));
    return Results.Created($"/api/tasks/{task.Id}", task);
});

api.MapPatch("/{id:guid}/complete", async (IMediator mediator, Guid id) =>
    await mediator.Send(new CompleteTaskCommand(id)) ? Results.Ok() : Results.NotFound());

api.MapPut("/{id:guid}", async (IMediator mediator, Guid id, EditTaskRequest req) =>
{
    if (!await mediator.Send(new EditTaskCommand(id, req.Title, req.Description, req.DueDate, req.Priority)))
        return Results.NotFound();
    return Results.Ok(await mediator.Send(new GetTaskByIdQuery(id)));
});

api.MapDelete("/{id:guid}", async (IMediator mediator, Guid id) =>
    await mediator.Send(new DeleteTaskCommand(id)) ? Results.Ok() : Results.NotFound());

app.Run();

internal record CreateTaskRequest(string Title, string? Description, DateTime? DueDate, Priority Priority);
internal record EditTaskRequest(string? Title, string? Description, DateTime? DueDate, Priority? Priority);
