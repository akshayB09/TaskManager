# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the app (serves on http://localhost:5000)
cd TaskManager/TaskManager && dotnet run

# Build only
cd TaskManager/TaskManager && dotnet build

# Restore dependencies
cd TaskManager/TaskManager && dotnet restore

# Add a new EF Core migration after changing the model
cd TaskManager/TaskManager && dotnet ef migrations add <MigrationName>

# Apply pending migrations to the database
cd TaskManager/TaskManager && dotnet ef database update
```

There are no tests in this project currently.

## Architecture

This is an ASP.NET Core 10 minimal-API web app that also serves a vanilla JS frontend as static files.

**Request flow:**
- `/` and static assets → served from `wwwroot/` via `UseStaticFiles`
- `/api/tasks/*` → handled by minimal API endpoints defined inline in `Program.cs`

**Backend layers:**
- `Program.cs` — all route definitions and request/response records (`CreateTaskRequest`, `EditTaskRequest`). No controllers. Registers `TaskDbContext` (Scoped) and `TaskService` (Scoped).
- `Services/TaskService.cs` — scoped service injected with `TaskDbContext`. All mutations call `context.SaveChanges()`. Public method signatures: `GetAll`, `GetById`, `FindByPrefix`, `Add`, `Complete`, `Delete`, `Edit`.
- `Data/TaskDbContext.cs` — EF Core `DbContext` with a single `DbSet<TaskItem> Tasks`. Configures `Priority` enum to store as a string (`"Low"` / `"Medium"` / `"High"`) via `EnumToStringConverter`.
- `Models/TaskItem.cs` — single model with a `Priority` enum (`Low`, `Medium`, `High`). Enums are serialized as strings in both JSON responses (configured in `Program.cs`) and the database (configured in `TaskDbContext`).
- `Migrations/` — EF Core migration history. `InitialCreate` sets up the `Tasks` table.

**Frontend (`wwwroot/`):**
- `app.js` — all UI logic. Fetches from `/api/tasks`, renders task cards, shows a live pending-task count in the header, handles add/edit modal, filter buttons, and event delegation for complete/edit/delete actions. Tasks are sorted by priority then due date client-side.
- `index.html` + `styles.css` — single-page layout with a modal for add/edit.

**Data persistence:**
- Tasks are stored in a SQLite database at `~/.taskmanager/tasks.db`.
- `TaskDbContext` is registered as Scoped — one instance per HTTP request, which is EF Core's designed lifetime. `TaskService` is also Scoped so it doesn't outlive the context it depends on.
- If you change `TaskItem`, add a new EF migration rather than editing existing ones.
