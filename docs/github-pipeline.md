# GitHub Actions Build Pipeline — Beginner Guide

This guide explains how the CI/CD pipeline works for this project, what every part does, and how to set it up from scratch.

---

## What is a CI/CD Pipeline?

**CI (Continuous Integration)** — Every time you push code, it is automatically built and checked. If the build fails, you know immediately before anything is merged.

**CD (Continuous Delivery)** — Every time code is merged to `main`, it is automatically deployed to your live environment (Azure in this case).

Without a pipeline, you would have to manually build and deploy every time — it's error-prone and slow. A pipeline does it for you automatically.

---

## How it works in this project

```
Developer pushes code
        │
        ▼
GitHub detects the push
        │
        ▼
GitHub Actions runs the workflow
        │
        ├── Restore NuGet packages
        ├── Build the project
        │       │
        │       ├── Build failed? → PR is blocked, you get notified
        │       └── Build passed? → PR can be merged
        │
        └── (Only on merge to main)
                ├── Publish the app
                ├── Login to Azure
                └── Deploy to Azure App Service
```

---

## The Workflow File

The pipeline is defined in [.github/workflows/build.yml](../.github/workflows/build.yml).

```yaml
name: Build and Deploy

on:
  push:
    branches:
      - '**'          # Run on every branch push
  pull_request:
    branches:
      - main          # Run on every PR targeting main

jobs:
  build:
    runs-on: ubuntu-latest    # GitHub spins up a fresh Linux VM for every run

    steps:
      - uses: actions/checkout@v4         # Download the repo code onto the VM

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'          # Install .NET 10 on the VM

      - name: Restore dependencies
        run: dotnet restore TaskManager/TaskManager.sln   # Download NuGet packages

      - name: Build
        run: dotnet build TaskManager/TaskManager.sln --no-restore --configuration Release

      - name: Publish
        if: github.ref == 'refs/heads/main' && github.event_name == 'push'
        run: dotnet publish TaskManager/TaskManager.csproj --configuration Release --output ./publish

      - name: Login to Azure
        if: github.ref == 'refs/heads/main' && github.event_name == 'push'
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}   # Uses the secret stored in GitHub

      - name: Deploy to Azure App Service
        if: github.ref == 'refs/heads/main' && github.event_name == 'push'
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_WEBAPP_NAME }}
          package: ./publish
```

### Key concepts in the file

| Concept | What it means |
|---|---|
| `on: push` | The event that triggers the workflow |
| `jobs` | A group of steps that run together on one VM |
| `runs-on: ubuntu-latest` | GitHub provides a fresh Linux VM for free |
| `uses: actions/checkout@v4` | A pre-built action from GitHub's marketplace |
| `if: github.ref == 'refs/heads/main'` | Only run this step when the branch is `main` |
| `${{ secrets.AZURE_CREDENTIALS }}` | A secret value stored in GitHub, never visible in logs |

---

## Branch Protection Rules

The `main` branch is protected. This means:

- You **cannot push directly** to `main`
- Every change must go through a **Pull Request**
- The PR cannot be merged until the **build check passes**
- The branch must be **up to date** with `main` before merging

This prevents broken code from ever reaching production.

### How to set branch protection (one-time setup)

```bash
gh api repos/<your-username>/<your-repo>/branches/main/protection \
  --method PUT \
  --input - <<'EOF'
{
  "required_status_checks": {
    "strict": true,
    "contexts": ["build"]
  },
  "enforce_admins": false,
  "required_pull_request_reviews": null,
  "restrictions": null
}
EOF
```

---

## GitHub Secrets

Secrets are encrypted values stored in GitHub. The pipeline uses them to authenticate with Azure without exposing credentials in the code.

### Secrets used in this project

| Secret name | What it contains |
|---|---|
| `AZURE_CREDENTIALS` | Service principal JSON for Azure login |
| `AZURE_WEBAPP_NAME` | The name of the Azure App Service (`taskmanager-akshay`) |

### How to add a secret

1. Go to your repo on GitHub
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Enter the name and value → **Save**

Or via CLI:
```bash
gh secret set SECRET_NAME --repo <owner>/<repo> --body "secret-value"
```

---

## Day-to-day Developer Workflow

```
1. Create a Jira ticket for your work (e.g. TM-31)

2. Create a feature branch:
   git checkout -b feature/TM-31-short-description

3. Make your changes and commit:
   git commit -m "TM-31: Short description of change"

4. Push the branch:
   git push origin feature/TM-31-short-description

5. Open a Pull Request on GitHub targeting main
   - Include "Closes TM-31" in the PR description

6. Wait for the build check to pass (green tick)

7. Merge the PR → triggers automatic deploy to Azure
```

---

## Reading Pipeline Results

After pushing, go to **Actions** tab on GitHub to see the run.

| Status | Icon | Meaning |
|---|---|---|
| In progress | Yellow circle | Currently running |
| Passed | Green tick | Build succeeded |
| Failed | Red X | Something broke — click to see logs |

### Common failure reasons

| Error | Likely cause | Fix |
|---|---|---|
| `dotnet restore` fails | Missing package or wrong .NET version | Check `.csproj` dependencies |
| `dotnet build` fails | Compile error in code | Fix the code error shown in logs |
| `Login to Azure` fails | Invalid `AZURE_CREDENTIALS` secret | Recreate the service principal |
| `Deploy to Azure` fails | App Service not found | Check `AZURE_WEBAPP_NAME` secret value |

---

## Useful CLI Commands

```bash
# List recent pipeline runs
gh run list --repo <owner>/<repo>

# Watch a run live
gh run watch <run-id> --repo <owner>/<repo>

# View logs of a failed run
gh run view <run-id> --repo <owner>/<repo> --log-failed

# List GitHub secrets (names only, not values)
gh secret list --repo <owner>/<repo>
```
