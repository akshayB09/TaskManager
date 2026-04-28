using System.Text.Json.Serialization;
using TaskManager.Models;
using TaskManager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TaskService>();

// Use string enum values ("High") instead of numbers (2) in JSON responses
builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseDefaultFiles();   // serves wwwroot/index.html for "/"
app.UseStaticFiles();    // serves wwwroot/ files

var api = app.MapGroup("/api/tasks");

api.MapGet("/", (TaskService svc) => svc.GetAll());

api.MapPost("/", (TaskService svc, CreateTaskRequest req) =>
{
    var task = svc.Add(req.Title, req.Description ?? "", req.DueDate, req.Priority);
    return Results.Created($"/api/tasks/{task.Id}", task);
});

api.MapPatch("/{id:guid}/complete", (TaskService svc, Guid id) =>
    svc.Complete(id) ? Results.Ok() : Results.NotFound());

api.MapPut("/{id:guid}", (TaskService svc, Guid id, EditTaskRequest req) =>
{
    if (!svc.Edit(id, req.Title, req.Description, req.DueDate, req.Priority))
        return Results.NotFound();
    return Results.Ok(svc.GetById(id));
});

api.MapDelete("/{id:guid}", (TaskService svc, Guid id) =>
    svc.Delete(id) ? Results.Ok() : Results.NotFound());

app.Run();

record CreateTaskRequest(string Title, string? Description, DateTime? DueDate, Priority Priority);
record EditTaskRequest(string? Title, string? Description, DateTime? DueDate, Priority? Priority);
