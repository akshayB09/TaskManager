# Azure App Service Deployment — Beginner Guide

This guide explains how the TaskManager app is hosted on Azure, what every resource does, and how to set it up from scratch at zero cost.

---

## What is Azure App Service?

Azure App Service is Microsoft's platform for hosting web apps in the cloud. You give it your built app and it runs it for you — no need to manage servers, networking, or operating systems.

For this project we use the **Free F1 tier** which costs **$0/month**.

---

## Azure Resources Created for this Project

```
Azure Subscription (your account)
└── Resource Group: taskmanager-rg        ← a folder that holds all resources
    └── App Service Plan: taskmanager-plan ← the server that runs your app (F1 Free)
        └── Web App: taskmanager-akshay   ← the actual running app
```

### What each resource does

| Resource | Name | Purpose |
|---|---|---|
| Resource Group | `taskmanager-rg` | A logical container — delete this to delete everything |
| App Service Plan | `taskmanager-plan` | Defines the server size and pricing tier (F1 = Free) |
| Web App | `taskmanager-akshay` | Hosts and runs your ASP.NET Core app |

---

## Prerequisites

- An Azure account — sign up at https://azure.microsoft.com/free (free, requires a credit card for identity only)
- Azure CLI installed: `brew install azure-cli` (Mac) or https://learn.microsoft.com/en-us/cli/azure/install-azure-cli
- GitHub CLI installed: `brew install gh`

---

## Step-by-Step Setup

### Step 1 — Install Azure CLI and Login

```bash
brew install azure-cli
az login --use-device-code
```

This opens a browser login. Sign in with your Microsoft account.

Verify you're logged in:
```bash
az account show
```

You should see your subscription name and ID.

---

### Step 2 — Create a Resource Group

A resource group is like a project folder — it holds all Azure resources for this app.

```bash
az group create \
  --name taskmanager-rg \
  --location australiaeast
```

> **Location options:** `australiaeast`, `eastus`, `westeurope`, `southeastasia`
> Pick the region closest to your users.

---

### Step 3 — Create an App Service Plan (Free Tier)

The App Service Plan defines the compute resources. `F1` is the free tier.

```bash
az appservice plan create \
  --name taskmanager-plan \
  --resource-group taskmanager-rg \
  --sku F1 \
  --is-linux
```

**Important:** Always specify `--sku F1` explicitly. The portal default is often B1 (Basic) which costs ~$13/month.

Verify you're on the free tier:
```bash
az appservice plan show \
  --name taskmanager-plan \
  --resource-group taskmanager-rg \
  --query sku.name
```

Output should be: `"F1"`

---

### Step 4 — Create the Web App

```bash
az webapp create \
  --name taskmanager-akshay \
  --resource-group taskmanager-rg \
  --plan taskmanager-plan \
  --runtime "DOTNETCORE:10.0"
```

> **App name must be globally unique** — if `taskmanager-akshay` is taken, choose another name and update the `AZURE_WEBAPP_NAME` GitHub secret accordingly.

Your app URL will be:
```
https://taskmanager-akshay.azurewebsites.net
```

---

### Step 5 — Create a Service Principal for GitHub Actions

A service principal is like a robot user that GitHub Actions uses to log into Azure and deploy your app. It has limited permissions — only access to this resource group.

```bash
az ad sp create-for-rbac \
  --name "taskmanager-github-deploy" \
  --role contributor \
  --scopes /subscriptions/$(az account show --query id -o tsv)/resourceGroups/taskmanager-rg \
  --json-auth
```

This outputs a JSON block like:
```json
{
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "...",
  "tenantId": "...",
  ...
}
```

**Copy the entire JSON output** — you need it in the next step.

---

### Step 6 — Add Secrets to GitHub

Store the service principal JSON and app name as GitHub secrets so the pipeline can use them.

```bash
# Add the service principal credentials
gh secret set AZURE_CREDENTIALS \
  --repo <your-github-username>/<your-repo> \
  --body '<paste the full JSON here>'

# Add the web app name
gh secret set AZURE_WEBAPP_NAME \
  --repo <your-github-username>/<your-repo> \
  --body "taskmanager-akshay"
```

Verify the secrets are set:
```bash
gh secret list --repo <your-github-username>/<your-repo>
```

---

### Step 7 — Deploy

Once the secrets are set, merging any PR to `main` automatically triggers a deploy via GitHub Actions.

To trigger a manual deploy without a code change:
```bash
gh workflow run "Build and Deploy" --repo <owner>/<repo> --ref main
```

---

## How Deployment Works

```
GitHub Actions (on merge to main)
        │
        ▼
dotnet publish                   ← compiles and packages the app
        │
        ▼
az login (service principal)     ← authenticates with Azure
        │
        ▼
azure/webapps-deploy             ← uploads the package to App Service
        │
        ▼
App Service restarts with new code
        │
        ▼
https://taskmanager-akshay.azurewebsites.net is updated
```

---

## Verifying the Deployment

Check the app is responding:
```bash
curl -s -o /dev/null -w "%{http_code}" https://taskmanager-akshay.azurewebsites.net
```

Output `200` means the app is live.

View recent deployments in Azure:
```bash
az webapp deployment list --name taskmanager-akshay --resource-group taskmanager-rg
```

View app logs:
```bash
az webapp log tail --name taskmanager-akshay --resource-group taskmanager-rg
```

---

## About the Database (SQLite)

The app uses SQLite — a file-based database stored at `~/.taskmanager/tasks.db`.

| Environment | Database location | Data persists? |
|---|---|---|
| Local machine | `~/.taskmanager/tasks.db` on your Mac | Yes |
| Azure Free tier | `/home/.taskmanager/tasks.db` on Azure VM | No — wiped on restart |

**Data on Azure is not synced with your local machine.** They are completely separate databases.

For production apps with real users, you would replace SQLite with a cloud database like Azure SQL or PostgreSQL. For a personal/demo app on the free tier, SQLite is fine as long as you accept that data may be lost on restarts.

---

## Keeping Costs at Zero

The F1 App Service plan is free forever with no time limit. The only ways you can accidentally get charged:

1. **Creating a paid resource** — always check the SKU when creating resources
2. **Azure free account credit expiry** — after 30 days the $200 credit expires, but F1 stays free
3. **Creating additional resources** (Azure SQL, Storage, etc.) — these are paid

### Set a budget alert to be safe

In Azure Portal:
1. Search for **Cost Management**
2. Click **Budgets** → **Add**
3. Set amount to **$1** and add your email
4. You'll be emailed before anything is charged

---

## Tearing Down (Deleting Everything)

To remove all Azure resources and ensure zero ongoing cost:

```bash
az group delete --name taskmanager-rg --yes
```

This deletes the resource group and everything inside it (App Service Plan + Web App).

---

## Useful CLI Commands

```bash
# Check your Azure login
az account show

# List all resource groups
az group list --output table

# List all web apps
az webapp list --output table

# Check which pricing tier your plan is on
az appservice plan show \
  --name taskmanager-plan \
  --resource-group taskmanager-rg \
  --query sku

# Restart the web app
az webapp restart --name taskmanager-akshay --resource-group taskmanager-rg

# Stream live logs from the app
az webapp log tail --name taskmanager-akshay --resource-group taskmanager-rg

# Check current monthly cost estimate
az consumption usage list --output table
```
