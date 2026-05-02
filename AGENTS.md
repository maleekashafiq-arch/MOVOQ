# AGENTS.md

## Cursor Cloud specific instructions

### Overview

MOVOQ is a .NET 10 Clean Architecture backend (ASP.NET Core Web API). It is in early scaffolding phase — a single API service with no database configured yet.

### Prerequisites

The .NET 10 SDK is installed via `dotnet-install.sh` to `$HOME/.dotnet`. The update script handles this automatically. Environment variables `DOTNET_ROOT` and `PATH` are set in `~/.bashrc`.

### Key commands

| Task | Command |
|---|---|
| Restore packages | `dotnet restore MOVOQ.slnx` |
| Build | `dotnet build MOVOQ.slnx` |
| Run tests | `dotnet test MOVOQ.slnx` |
| Run API (dev) | `dotnet run --project MOVOQ.Api --urls "http://localhost:5000"` |

### Notes

- The solution uses `.slnx` format (not `.sln`). Use `MOVOQ.slnx` for all dotnet CLI commands.
- The API exposes a sample `/weatherforecast` endpoint and OpenAPI docs at `/openapi/v1.json`.
- No database, Docker, or external services are required at this stage.
- HTTPS redirection is configured but the dev server should be started with HTTP (`http://localhost:5000`) to avoid certificate issues in cloud environments.
