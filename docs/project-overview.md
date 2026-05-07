# TaskManager — Project Overview

A complete guide to understanding, running, and contributing to this project. Written for newcomers with no prior knowledge of the codebase.

---

## Table of Contents

1. [What is this app?](#1-what-is-this-app)
2. [Technology stack](#2-technology-stack)
3. [Folder structure](#3-folder-structure)
4. [Architecture overview](#4-architecture-overview)
5. [Domain layer](#5-domain-layer)
6. [Application layer — CQRS with MediatR](#6-application-layer--cqrs-with-mediatr)
7. [Infrastructure layer — Database](#7-infrastructure-layer--database)
8. [Presentation layer — Blazor UI](#8-presentation-layer--blazor-ui)
9. [API endpoints](#9-api-endpoints)
10. [Authentication — Azure AD](#10-authentication--azure-ad)
11. [Configuration and secrets](#11-configuration-and-secrets)
12. [CI/CD pipeline](#12-cicd-pipeline)
13. [Running locally](#13-running-locally)
14. [Deploying to Azure](#14-deploying-to-azure)
15. [Making changes — common tasks](#15-making-changes--common-tasks)

---

## 1. What is this app?

TaskManager is a web application for managing personal tasks. Users can:

- Create tasks with a title, description, due date, and priority (Low / Medium / High)
- Mark tasks as complete
- Edit or delete tasks
- Filter tasks by status (Pending / All / Done)

Access is protected by Microsoft sign-in — only authenticated users can use the app.

---

## 2. Technology stack

| Layer | Technology | Why |
|---|---|---|
| Framework | ASP.NET Core 10 | Backend and hosting |
| UI | Blazor Server | Interactive UI without writing JavaScript |
| Database | SQLite via EF Core | Simple file-based database, no server needed |
| Auth | Microsoft Entra ID (Azure AD) | Microsoft sign-in, no password management |
| Pattern | CQRS with MediatR | Keeps business logic clean and separated |
| Hosting | Azure App Service | Cloud hosting with free tier available |
| CI/CD | GitHub Actions | Automated build and deploy on every push |

---

## 3. Folder structure

```
TaskManager/
├── TaskManager/                    ← main project
│   ├── Program.cs                  ← app entry point, all wiring
│   ├── appsettings.json            ← config (no secrets here)
│   ├── TaskManager.csproj          ← NuGet packages and project settings
│   │
│   ├── Domain/                     ← core business concepts
│   │   ├── Entities/TaskItem.cs    ← the Task model
│   │   └── Enums/Priority.cs      ← Low / Medium / High
│   │
│   ├── Application/                ← business logic (CQRS)
│   │   ├── Common/
│   │   │   └── ITaskRepository.cs  ← interface (contract) for data access
│   │   └── Tasks/
│   │       ├── Commands/           ← write operations (Add, Edit, Complete, Delete)
│   │       └── Queries/            ← read operations (GetAll, GetById)
│   │
│   ├── Infrastructure/             ← database implementation
│   │   └── Persistence/
│   │       ├── TaskDbContext.cs    ← EF Core database context
│   │       └── TaskRepository.cs  ← concrete implementation of ITaskRepository
│   │
│   ├── Migrations/                 ← EF Core database migrations (auto-generated)
│   │
│   ├── Components/                 ← Blazor UI
│   │   ├── App.razor               ← HTML shell
│   │   ├── Routes.razor            ← routing with auth protection
│   │   ├── _Imports.razor          ← shared @using statements
│   │   ├── Layout/
│   │   │   └── MainLayout.razor   ← page layout with sign in/out
│   │   └── Pages/
│   │       └── Home.razor          ← main task management page
│   │
│   ├── Properties/
│   │   └── launchSettings.json    ← local dev server settings
│   └── wwwroot/
│       └── styles.css              ← global CSS
│
└── docs/                           ← documentation
    ├── project-overview.md         ← this file
    ├── authentication.md           ← Azure AD setup guide
    ├── azure-setup.md              ← Azure App Service setup
    └── github-pipeline.md          ← CI/CD pipeline guide
```

---

## 4. Architecture overview

The app is split into four layers. Each layer only talks to the layer below it — this keeps the code clean and easy to change.

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│   Blazor UI (Components/)               │
│   REST API endpoints (Program.cs)       │
└────────────────┬────────────────────────┘
                 │ sends Commands/Queries via MediatR
┌────────────────▼────────────────────────┐
│         Application Layer               │
│   Commands — write operations           │
│   Queries  — read operations            │
│   ITaskRepository — data contract       │
└────────────────┬────────────────────────┘
                 │ calls interface
┌────────────────▼────────────────────────┐
│         Infrastructure Layer            │
│   TaskRepository — database queries     │
│   TaskDbContext  — EF Core / SQLite     │
└────────────────┬────────────────────────┘
                 │ reads/writes
┌────────────────▼────────────────────────┐
│         Domain Layer                    │
│   TaskItem — the core data model        │
│   Priority — Low / Medium / High        │
└─────────────────────────────────────────┘
```

**Why this structure?**
If you ever swap SQLite for PostgreSQL, you only change the Infrastructure layer. The business logic and UI don't need to change at all.

---

## 5. Domain layer

**Files:** [Domain/Entities/TaskItem.cs](../TaskManager/Domain/Entities/TaskItem.cs), [Domain/Enums/Priority.cs](../TaskManager/Domain/Enums/Priority.cs)

This is the heart of the app — pure C# with no dependencies on databases, HTTP, or anything external.

### TaskItem

The single model that represents a task:

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Unique identifier, auto-generated |
| `Title` | `string` | Short name for the task (required) |
| `Description` | `string` | Optional details |
| `DueDate` | `DateTime?` | Optional deadline |
| `Priority` | `Priority` | Low, Medium, or High |
| `IsCompleted` | `bool` | Whether the task is done |
| `CreatedAt` | `DateTime` | When the task was created |

### Priority enum

```csharp
public enum Priority { Low, Medium, High }
```

Stored as the string `"Low"`, `"Medium"`, or `"High"` in the database (not as a number) so the data is human-readable.

---

## 6. Application layer — CQRS with MediatR

**Files:** [Application/](../TaskManager/Application/)

### What is CQRS?

CQRS stands for **Command Query Responsibility Segregation**. It means:
- **Commands** — operations that *change* data (add, edit, complete, delete)
- **Queries** — operations that *read* data (get all, get by id)

Each operation is a separate class. This makes the code easy to find and easy to test.

### What is MediatR?

MediatR is a library that acts as a message bus. Instead of calling a service directly, the UI sends a message (command or query) and MediatR finds the right handler to process it.

```
UI sends:  new AddTaskCommand("Buy milk", ...)
MediatR finds: AddTaskHandler
Handler calls: repo.AddAsync(...)
Returns: the new TaskItem
```

### Commands (write operations)

| Command | What it does |
|---|---|
| `AddTaskCommand` | Creates a new task |
| `EditTaskCommand` | Updates title, description, due date, or priority |
| `CompleteTaskCommand` | Marks a task as done |
| `DeleteTaskCommand` | Removes a task permanently |

### Queries (read operations)

| Query | What it returns |
|---|---|
| `GetAllTasksQuery` | All tasks as a list |
| `GetTaskByIdQuery` | A single task by its ID |

### ITaskRepository

```csharp
public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<TaskItem> AddAsync(...);
    Task<bool> CompleteAsync(Guid id);
    Task<bool> EditAsync(...);
    Task<bool> DeleteAsync(Guid id);
}
```

This is a contract — it defines *what* data operations are available without saying *how* they work. The Application layer only knows about this interface, not the database.

---

## 7. Infrastructure layer — Database

**Files:** [Infrastructure/Persistence/](../TaskManager/Infrastructure/Persistence/)

### TaskDbContext

The EF Core `DbContext` — the bridge between C# objects and the SQLite database.

```csharp
public DbSet<TaskItem> Tasks => Set<TaskItem>();
```

Two important configurations:
- `Id` (Guid) is stored as TEXT in SQLite
- `Priority` (enum) is stored as a string (`"Low"`, `"Medium"`, `"High"`) not a number

### TaskRepository

The concrete implementation of `ITaskRepository`. All database queries live here.

Key things to know:
- Uses `AsNoTracking()` on reads for better performance (EF Core won't track changes we don't intend to save)
- Calls `context.ChangeTracker.Clear()` before write operations to avoid stale entity conflicts
- Calls `context.SaveChangesAsync()` after every mutation

### Database location

The SQLite database is stored at:
```
~/.taskmanager/tasks.db
```

The directory is created automatically on first run. The file persists between app restarts.

### Migrations

EF Core migrations track changes to the database schema. The `InitialCreate` migration created the `Tasks` table.

**If you add a new property to `TaskItem`:**
```bash
cd TaskManager/TaskManager
dotnet ef migrations add <DescriptiveName>
dotnet ef database update
```

Never edit existing migration files — always add a new one.

---

## 8. Presentation layer — Blazor UI

**Files:** [Components/](../TaskManager/Components/)

### What is Blazor Server?

Blazor Server lets you write interactive UI in C# instead of JavaScript. The UI runs on the server over a persistent WebSocket connection (SignalR). When you click a button, the event is sent to the server, the server updates state, and the diff is sent back to the browser.

### File breakdown

#### App.razor
The HTML shell — sets up the `<head>`, loads CSS, and bootstraps Blazor. You rarely need to touch this.

#### Routes.razor
Handles navigation and auth enforcement:
- `CascadingAuthenticationState` — makes the signed-in user available throughout the whole app
- `AuthorizeRouteView` — checks `[Authorize]` on each page before rendering it. If not signed in, shows a "Sign in" link instead.

#### _Imports.razor
Shared `@using` statements so you don't repeat them in every file. Adding a namespace here makes it available in all Razor components.

#### Layout/MainLayout.razor
The page wrapper rendered around every page. Contains the Sign in / Sign out bar at the top using `AuthorizeView`:
- Signed in → shows the user's name and a Sign out link
- Not signed in → shows a Sign in link

#### Pages/Home.razor
The main (and only) page. Protected with `@attribute [Authorize]`.

What it does:
- Loads all tasks on first render via `GetAllTasksQuery`
- Renders task cards sorted by priority then due date
- Filter buttons (Pending / All / Done) filter client-side — no extra server calls
- Add / Edit modal — a single modal reused for both operations
- Complete / Edit / Delete buttons trigger commands via MediatR then reload the list

---

## 9. API endpoints

All endpoints are defined in [Program.cs](../TaskManager/Program.cs) and grouped under `/api/tasks`. Every endpoint requires authentication.

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/tasks` | Returns all tasks |
| `POST` | `/api/tasks` | Creates a new task |
| `PATCH` | `/api/tasks/{id}/complete` | Marks a task as complete |
| `PUT` | `/api/tasks/{id}` | Edits a task |
| `DELETE` | `/api/tasks/{id}` | Deletes a task |

### Request bodies

**POST `/api/tasks`**
```json
{
  "title": "Buy milk",
  "description": "From the corner shop",
  "dueDate": "2026-05-10T00:00:00",
  "priority": "High"
}
```

**PUT `/api/tasks/{id}`**
```json
{
  "title": "Buy oat milk",
  "description": null,
  "dueDate": null,
  "priority": "Medium"
}
```
All fields are optional — only non-null fields are updated.

---

## 10. Authentication — Azure AD

**Full guide:** [authentication.md](authentication.md)

The app uses **Microsoft Entra ID (Azure AD)** — users sign in with their Microsoft account. No passwords are stored in the app.

### How the sign-in flow works

```
1. User opens the app
2. Not signed in → AuthorizeRouteView redirects to /MicrosoftIdentity/Account/SignIn
3. User is redirected to Microsoft login page
4. User signs in with Microsoft account
5. Microsoft redirects back to /signin-oidc with a short-lived auth code
6. Microsoft.Identity.Web exchanges the code for tokens
7. An encrypted cookie is set in the browser
8. User is now authenticated — can access all pages and API endpoints
```

### Key packages

- `Microsoft.Identity.Web` — handles all OIDC token validation and cookie management
- `Microsoft.Identity.Web.UI` — provides the `/MicrosoftIdentity/Account/SignIn` and `SignOut` controller routes

### Config in appsettings.json

```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "TenantId": "YOUR_TENANT_ID",
  "ClientId": "YOUR_CLIENT_ID",
  "ClientSecret": "USE_USER_SECRETS_OR_KEYVAULT",
  "CallbackPath": "/signin-oidc"
}
```

**Never put real values in `appsettings.json`** — use user-secrets locally and Azure App Service environment variables in production.

---

## 11. Configuration and secrets

### appsettings.json

Checked into git. Contains structure and safe defaults — no real secrets.

### User secrets (local development only)

Stored on your machine at `~/.microsoft/usersecrets/<id>/secrets.json`. Never committed to git.

```bash
dotnet user-secrets set "AzureAd:TenantId" "<value>"
dotnet user-secrets set "AzureAd:ClientId" "<value>"
dotnet user-secrets set "AzureAd:ClientSecret" "<value>"
```

### Azure App Service environment variables (production)

Set in **Azure portal → App Service → Environment variables**. Use double underscore to represent nested JSON keys:

| Environment variable | Maps to |
|---|---|
| `AzureAd__TenantId` | `AzureAd.TenantId` |
| `AzureAd__ClientId` | `AzureAd.ClientId` |
| `AzureAd__ClientSecret` | `AzureAd.ClientSecret` |

---

## 12. CI/CD pipeline

**Full guide:** [github-pipeline.md](github-pipeline.md)

Every push to `main` triggers a GitHub Actions workflow that:

1. **Build** — restores packages and compiles the project
2. **Deploy** — publishes to Azure App Service using a service principal

### GitHub secrets required

| Secret | Description |
|---|---|
| `AZURE_CREDENTIALS` | Service principal JSON for Azure login |
| `AZURE_WEBAPP_NAME` | `taskmanager-akshay` |

### Branch strategy

- Work on a feature branch (e.g. `feature/my-change`)
- Open a PR to `main`
- Pipeline runs on the PR to verify the build passes
- Merge → pipeline deploys to Azure automatically

---

## 13. Running locally

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- An Azure AD app registration (see [authentication.md](authentication.md))

### First-time setup

```bash
# 1. Clone the repo
git clone https://github.com/akshayB09/TaskManager.git
cd TaskManager

# 2. Trust the HTTPS dev certificate
dotnet dev-certs https --trust

# 3. Set secrets
cd TaskManager
dotnet user-secrets set "AzureAd:TenantId" "<your-tenant-id>"
dotnet user-secrets set "AzureAd:ClientId" "<your-client-id>"
dotnet user-secrets set "AzureAd:ClientSecret" "<your-client-secret>"
```

### Running the app

```bash
cd TaskManager/TaskManager
dotnet run --launch-profile https
```

Open **`https://localhost:5001`** in your browser.

### Other useful commands

```bash
# Build only (check for errors without running)
dotnet build

# Restore NuGet packages
dotnet restore

# Add a new EF Core migration after changing TaskItem
dotnet ef migrations add <MigrationName>

# Apply pending migrations manually
dotnet ef database update
```

---

## 14. Deploying to Azure

**Full guide:** [azure-setup.md](azure-setup.md)

The app deploys automatically via GitHub Actions on every merge to `main`.

**To deploy manually:**
```bash
dotnet publish -c Release -o ./publish
az webapp deploy --resource-group <rg> --name taskmanager-akshay --src-path ./publish
```

**Production URL:** `https://taskmanager-akshay.azurewebsites.net`

---

## 15. Making changes — common tasks

### Add a new field to a task

1. Add the property to [Domain/Entities/TaskItem.cs](../TaskManager/Domain/Entities/TaskItem.cs)
2. Add the field to the relevant Commands/Queries in `Application/`
3. Update `TaskRepository` in `Infrastructure/` if needed
4. Add a new EF Core migration:
   ```bash
   dotnet ef migrations add Add<FieldName>ToTask
   ```
5. Update the Blazor UI in [Components/Pages/Home.razor](../TaskManager/Components/Pages/Home.razor)

### Add a new page

1. Create a new `.razor` file in `Components/Pages/`
2. Add `@page "/your-route"` at the top
3. Add `@attribute [Authorize]` to protect it
4. Add a link to it in `MainLayout.razor` if needed

### Add a new API endpoint

1. Open [Program.cs](../TaskManager/Program.cs)
2. Add your route inside the `api` group:
   ```csharp
   api.MapGet("/my-route", (IMediator mediator) => mediator.Send(new MyQuery()));
   ```
3. Create the corresponding Command or Query in `Application/Tasks/`

### Change the database schema

Always add a **new migration** — never edit existing ones:
```bash
dotnet ef migrations add <DescriptiveName>
dotnet ef database update
```
