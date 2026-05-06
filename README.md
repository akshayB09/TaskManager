# Task Manager

A full-stack task management web app built with **ASP.NET Core 10** (minimal API + **Blazor Server**), following **CQRS** and **clean architecture** principles. Tasks persist in a **SQLite** database via **Entity Framework Core 10**.

## Features

- Add, edit, complete, and delete tasks
- Priority levels (High / Medium / Low) with colour-coded cards
- Optional due dates
- Filter by Pending / All / Done
- Live pending task count in the header
- Data persists across restarts in a local SQLite database

## How to run

**Prerequisites:** .NET 10 SDK

```bash
git clone https://github.com/akshayB09/TaskManager.git
cd TaskManager/TaskManager
dotnet run
```

Open [http://localhost:5000](http://localhost:5000) in your browser.

The SQLite database is created automatically at `~/.taskmanager/tasks.db` on first run — no setup needed.

## Architecture

The project uses **CQRS** (Command Query Responsibility Segregation) with **MediatR** as the mediator bus, organized into three logical layers within a single `.csproj`.

```
TaskManager/
├── Domain/
│   ├── Entities/
│   │   └── TaskItem.cs              # Core entity, no framework dependencies
│   └── Enums/
│       └── Priority.cs              # Low / Medium / High
│
├── Application/
│   ├── Common/
│   │   └── ITaskRepository.cs       # Repository interface (owned by Application)
│   └── Tasks/
│       ├── Commands/
│       │   ├── AddTaskCommand.cs
│       │   ├── EditTaskCommand.cs
│       │   ├── CompleteTaskCommand.cs
│       │   └── DeleteTaskCommand.cs
│       └── Queries/
│           ├── GetAllTasksQuery.cs
│           └── GetTaskByIdQuery.cs
│
├── Infrastructure/
│   └── Persistence/
│       ├── TaskDbContext.cs          # EF Core DbContext, enum-to-string converter
│       └── TaskRepository.cs        # Implements ITaskRepository via EF Core + SQLite
│
├── Migrations/                      # EF Core migration history
│
├── Components/                      # Blazor Server UI
│   ├── Pages/Home.razor             # Task list, filters, add/edit modal
│   └── Layout/MainLayout.razor
│
├── wwwroot/styles.css
└── Program.cs                       # DI wiring, MediatR registration, minimal API endpoints
```

### Request flow

- **Blazor UI** injects `IMediator` and sends commands/queries directly — no service class in between.
- **REST API** (`/api/tasks/*`) endpoints receive requests, construct a command or query, and dispatch through `IMediator`.
- **Handlers** (in `Application/`) call `ITaskRepository` to read/write data.
- **`TaskRepository`** (in `Infrastructure/`) is the only place that touches EF Core.

### Dependency direction

```
Domain  ←  Application  ←  Infrastructure
                ↑
         Presentation (Blazor + Program.cs)
```

Domain and Application have no knowledge of Infrastructure or the web framework.

## What this project taught me

**CQRS and MediatR**
- Separating reads (queries) from writes (commands) makes each operation independently testable and gives a clear place to add cross-cutting concerns (logging, validation) via MediatR pipeline behaviours
- Co-locating the request record and its handler in one file is the pragmatic sweet spot for a small app — avoids file proliferation without losing the separation of concerns

**Clean architecture in a single project**
- Enforcing the dependency rule by folder convention (Domain → Application → Infrastructure) gives the structural benefits without the overhead of multiple `.csproj` files
- `ITaskRepository` living in Application (not Infrastructure) means handlers never import EF Core — swapping the database layer requires no Application changes

**Blazor Server**
- Blazor replaces the vanilla JS frontend with C# components that run on the server over a SignalR connection — no JSON serialization round-trips for UI state
- `@rendermode InteractiveServer` enables two-way data binding and event handling directly in Razor

**EF Core and SQLite**
- Enums stored as strings (`EnumToStringConverter`) are readable in the database and immune to reordering bugs
- Scoped `DbContext` lifetime (one per HTTP request) prevents change-tracker conflicts across concurrent requests
- All data access is async (`ToListAsync`, `SaveChangesAsync`) to avoid blocking the thread pool
