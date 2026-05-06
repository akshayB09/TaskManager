# Task Manager

## Quick Reference

| Resource | Link |
|----------|------|
| GitHub Repo | https://github.com/akshayB09/TaskManager |
| GitHub Actions | https://github.com/akshayB09/TaskManager/actions |
| Jira Board | https://claudetaskproject.atlassian.net/jira/software/projects/TM/boards |
| Jira Backlog | https://claudetaskproject.atlassian.net/jira/software/projects/TM/backlog |
| Atlassian Site | https://claudetaskproject.atlassian.net |

---

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

## CI / CD

The project uses **GitHub Actions** for continuous integration. The workflow file is at [.github/workflows/build.yml](.github/workflows/build.yml).

### What runs

| Step | Command |
|------|---------|
| Restore | `dotnet restore TaskManager/TaskManager.sln` |
| Build | `dotnet build TaskManager/TaskManager.sln --no-restore --configuration Release` |

### When it runs

- On every **push** to any branch (except `main`)
- On every **pull request** targeting `main`

### Branch protection

`main` is protected — a PR cannot be merged until the `build` check passes and the branch is up to date with `main`.

## Jira Issues

All tracked work for this project lives in the [Task Manager (TM)](https://claudetaskproject.atlassian.net/jira/software/projects/TM/boards) Jira project.

### Epics

| Key | Summary |
|-----|---------|
| [TM-1](https://claudetaskproject.atlassian.net/browse/TM-1) | Project Foundation |
| [TM-2](https://claudetaskproject.atlassian.net/browse/TM-2) | CLI Interface |
| [TM-3](https://claudetaskproject.atlassian.net/browse/TM-3) | Web UI |
| [TM-4](https://claudetaskproject.atlassian.net/browse/TM-4) | Persistence Layer |
| [TM-5](https://claudetaskproject.atlassian.net/browse/TM-5) | Deployment |

### Stories & Tasks

| Key | Type | Summary | Status |
|-----|------|---------|--------|
| [TM-6](https://claudetaskproject.atlassian.net/browse/TM-6) | Story | Test story | To Do |
| [TM-7](https://claudetaskproject.atlassian.net/browse/TM-7) | Story | Set up .NET project folder structure | To Do |
| [TM-8](https://claudetaskproject.atlassian.net/browse/TM-8) | Story | Create TaskItem model with all fields | To Do |
| [TM-9](https://claudetaskproject.atlassian.net/browse/TM-9) | Story | Define Priority enum (Low / Medium / High) | To Do |
| [TM-10](https://claudetaskproject.atlassian.net/browse/TM-10) | Story | Implement ICommand interface | To Do |
| [TM-11](https://claudetaskproject.atlassian.net/browse/TM-11) | Story | Implement add command | To Do |
| [TM-12](https://claudetaskproject.atlassian.net/browse/TM-12) | Story | Implement list command with filters | To Do |
| [TM-13](https://claudetaskproject.atlassian.net/browse/TM-13) | Story | Implement complete command | To Do |
| [TM-14](https://claudetaskproject.atlassian.net/browse/TM-14) | Story | Implement delete command | To Do |
| [TM-15](https://claudetaskproject.atlassian.net/browse/TM-15) | Story | Implement edit command | To Do |
| [TM-16](https://claudetaskproject.atlassian.net/browse/TM-16) | Story | Add colored console output via Printer | To Do |
| [TM-17](https://claudetaskproject.atlassian.net/browse/TM-17) | Story | Convert project to ASP.NET Core web app | To Do |
| [TM-18](https://claudetaskproject.atlassian.net/browse/TM-18) | Story | Create minimal API endpoints | To Do |
| [TM-19](https://claudetaskproject.atlassian.net/browse/TM-19) | Story | Build task list HTML page | To Do |
| [TM-20](https://claudetaskproject.atlassian.net/browse/TM-20) | Story | Add New Task modal with form | To Do |
| [TM-21](https://claudetaskproject.atlassian.net/browse/TM-21) | Story | Implement filter tabs (Pending / All / Done) | To Do |
| [TM-22](https://claudetaskproject.atlassian.net/browse/TM-22) | Story | Connect frontend to REST API with fetch() | To Do |
| [TM-23](https://claudetaskproject.atlassian.net/browse/TM-23) | Story | Implement JSON file persistence | To Do |
| [TM-24](https://claudetaskproject.atlassian.net/browse/TM-24) | Story | Handle corrupted JSON gracefully | To Do |
| [TM-25](https://claudetaskproject.atlassian.net/browse/TM-25) | Story | Serialize Priority enum as string | To Do |
| [TM-26](https://claudetaskproject.atlassian.net/browse/TM-26) | Story | Publish as self-contained executable | To Do |
| [TM-27](https://claudetaskproject.atlassian.net/browse/TM-27) | Story | Add binary to system PATH | To Do |
| [TM-28](https://claudetaskproject.atlassian.net/browse/TM-28) | Story | Run web app as background server | To Do |
| [TM-29](https://claudetaskproject.atlassian.net/browse/TM-29) | Task | Add README with project overview | To Do |
| [TM-30](https://claudetaskproject.atlassian.net/browse/TM-30) | Task | GitHub & Jira Setup Guide — TaskManager Project | To Do |
