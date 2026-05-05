# Task Manager

A full-stack task management web app built with **ASP.NET Core 10** (minimal API) and a **vanilla JS** frontend. Tasks persist in a **SQLite** database via **Entity Framework Core 10**.

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

## Project structure

```
TaskManager/
├── Data/
│   └── TaskDbContext.cs        # EF Core DbContext, enum-to-string converter
├── Migrations/                 # EF Core migration history
├── Models/
│   └── TaskItem.cs             # Task model + Priority enum
├── Services/
│   └── TaskService.cs          # Business logic, talks to DbContext
├── wwwroot/
│   ├── index.html              # Single-page UI
│   ├── app.js                  # All frontend logic (no framework)
│   └── styles.css
└── Program.cs                  # Minimal API routes, DI registration
```

## What this project taught me

**EF Core and SQLite**
- How to set up `DbContext`, register it with `AddDbContext`, and wire up a SQLite provider
- Why enums should be stored as strings in the database (readable, immune to reordering) using `EnumToStringConverter`
- The difference between `EnsureCreated()` and migrations — and why migrations are worth the extra setup for any real project

**Dependency injection lifetimes**
- Why `DbContext` must be **Scoped** (one per HTTP request) and not Singleton — a shared context across requests causes tracking conflicts and thread-safety issues
- Why a Scoped service (`TaskService`) can't be held by a Singleton — ASP.NET Core enforces this with a captive dependency error at startup

**EF Core migrations**
- `dotnet ef migrations add` scans the app's DI container by running startup code — so `AddDbContext` must be registered before the tooling can find the context
- Generated migrations include `Up()` (apply) and `Down()` (rollback), plus a model snapshot that future migrations diff against

**Minimal API**
- Route grouping with `MapGroup`, inline route handlers, and record types for request bodies keep the surface area small without needing controllers
- Static files and API routes coexist cleanly with `UseStaticFiles` + `UseDefaultFiles`
