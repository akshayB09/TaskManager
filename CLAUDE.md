# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the app (serves on https://localhost:5001 / http://localhost:5000)
cd TaskManager/TaskManager && dotnet run

# Build only
cd TaskManager/TaskManager && dotnet build

# Restore dependencies
cd TaskManager/TaskManager && dotnet restore
```

There are no tests in this project currently.

## Architecture

This is an ASP.NET Core 10 minimal-API web app that also serves a vanilla JS frontend as static files.

**Request flow:**
- `/` and static assets → served from `wwwroot/` via `UseStaticFiles`
- `/api/tasks/*` → handled by minimal API endpoints defined inline in `Program.cs`

**Backend layers:**
- `Program.cs` — all route definitions and request/response records (`CreateTaskRequest`, `EditTaskRequest`). No controllers.
- `Services/TaskService.cs` — singleton that owns the in-memory task list and persists it to `~/.taskmanager/tasks.json` on every mutation. All reads come from the in-memory list; disk is only read at startup.
- `Models/TaskItem.cs` — single model with a `Priority` enum (`Low`, `Medium`, `High`). Enums are serialized as strings (configured in both `Program.cs` and `TaskService`).

**Frontend (`wwwroot/`):**
- `app.js` — all UI logic. Fetches from `/api/tasks`, renders task cards, handles add/edit modal, filter buttons, and event delegation for complete/edit/delete actions. Tasks are sorted by priority then due date client-side.
- `index.html` + `styles.css` — single-page layout with a modal for add/edit.

**Data persistence:**
- Tasks are stored at `~/.taskmanager/tasks.json` as a JSON array. A corrupted file is silently discarded and replaced with an empty list on next write.
- `TaskService` is registered as a singleton, so the in-memory list is shared across all requests.
