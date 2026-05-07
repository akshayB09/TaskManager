# Authentication — Azure AD (Microsoft Entra ID)

This app uses Microsoft Entra ID (Azure AD) for authentication via the `Microsoft.Identity.Web` library. Users must sign in with a Microsoft account before accessing the Task Manager.

---

## How it works

```
User visits app
  → not authenticated → redirected to Microsoft login
  → user signs in with Microsoft account
  → Microsoft redirects back to /signin-oidc with auth code
  → Microsoft.Identity.Web exchanges code for tokens
  → cookie set → user is authenticated
  → protected pages and /api/tasks/* become accessible
```

---

## Azure Portal Setup (one-time)

### 1. Register the app

1. Go to [portal.azure.com](https://portal.azure.com) → search **App registrations** → **New registration**
2. Name: `TaskManager` (or any name)
3. Supported account types: **Single tenant** (your org only) or **Multi-tenant**
4. Click **Register**

### 2. Copy these values

From the **Overview** page:

| Value | Used for |
|---|---|
| **Application (client) ID** | `AzureAd:ClientId` in config |
| **Directory (tenant) ID** | `AzureAd:TenantId` in config |

> Application (client) ID and Client ID are the same thing.

### 3. Create a client secret

1. Left sidebar → **Certificates & secrets** → **+ New client secret**
2. Set a description (e.g. `local-dev`) and expiry
3. Copy the **Value** immediately — it is only shown once
4. Used for `AzureAd:ClientSecret` in config

> Set a calendar reminder before the secret expiry date — a expired secret will silently break sign-in.

### 4. Add redirect URIs

1. Left sidebar → **Authentication**
2. Under **Web → Redirect URIs**, add:
   - `https://localhost:5001/signin-oidc` — local development
   - `https://<your-app>.azurewebsites.net/signin-oidc` — production
3. Under **Implicit grant**, check **ID tokens**
4. Click **Save**

---

## Local Development Setup

### 1. Trust the HTTPS dev certificate (one-time)

```bash
dotnet dev-certs https --trust
```

### 2. Store secrets (never commit these)

```bash
cd TaskManager
dotnet user-secrets init
dotnet user-secrets set "AzureAd:TenantId" "<your-tenant-id>"
dotnet user-secrets set "AzureAd:ClientId" "<your-client-id>"
dotnet user-secrets set "AzureAd:ClientSecret" "<your-client-secret>"
```

### 3. Run the app

```bash
dotnet run --launch-profile https
```

### 4. Open in browser

```
https://localhost:5001
```

You will be redirected to the Microsoft login page. After signing in, you land on the Task Manager.

---

## Production (Azure App Service)

Set these in **App Service → Configuration → Application settings** (double underscore = nested JSON):

| Name | Value |
|---|---|
| `AzureAd__TenantId` | your tenant ID |
| `AzureAd__ClientId` | your client ID |
| `AzureAd__ClientSecret` | your client secret |

Also add the production redirect URI in the Azure portal:
```
https://<your-app>.azurewebsites.net/signin-oidc
```

---

## What is protected

| Route | Protection |
|---|---|
| `/` (Blazor UI) | `[Authorize]` — redirects to Microsoft login if not signed in |
| `/api/tasks/*` | `RequireAuthorization()` — returns 401 if no valid session |

---

## Packages added

| Package | Purpose |
|---|---|
| `Microsoft.Identity.Web` | OIDC middleware and token validation |
| `Microsoft.Identity.Web.UI` | Provides `/MicrosoftIdentity/Account/SignIn` and `SignOut` controller routes |

---

## Files changed

| File | Change |
|---|---|
| `Program.cs` | Added auth services and middleware |
| `appsettings.json` | Added `AzureAd` config section (placeholder values) |
| `Components/_Imports.razor` | Added auth-related `@using` statements |
| `Components/Routes.razor` | Wrapped with `CascadingAuthenticationState`, switched to `AuthorizeRouteView` |
| `Components/Layout/MainLayout.razor` | Added Sign in / Sign out UI via `AuthorizeView` |
| `Components/Pages/Home.razor` | Added `@attribute [Authorize]` |
| `Properties/launchSettings.json` | Added HTTPS profile on port 5001 |
