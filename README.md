# HobbyApp Backend

REST API for the HobbyApp mobile client (Expo/React Native). It provides
authentication and per-user **Notes** (typed + handwritten) and **Tasks**
(checklists), built with ASP.NET Core and PostgreSQL following Clean Architecture.

## Tech stack

- **.NET 10** / ASP.NET Core Web API (controllers)
- **PostgreSQL** via **EF Core** (Npgsql)
- **ASP.NET Core Identity** + **JWT** (access + refresh tokens)
- **Docker Compose** for local PostgreSQL

## Architecture

Clean Architecture with dependencies pointing inward only:

```
HobbyApp.Api            ← controllers, DI composition root, JWT pipeline
   └─> HobbyApp.Infrastructure   ← EF Core DbContext, Identity, persistence
          └─> HobbyApp.Application   ← use cases (services), DTOs, interfaces
                 └─> HobbyApp.Domain    ← entities (no dependencies)
```

- The Application layer talks to persistence only through `IApplicationDbContext`.
- Identity lives in Infrastructure, exposed to the rest of the app via `IIdentityService` / `ICurrentUser`.

## Project structure

```
hobby-app-backend/
├── HobbyApp.slnx
├── docker-compose.yml          # local PostgreSQL
├── Dockerfile                  # multi-stage build of the API
└── src/
    ├── HobbyApp.Domain/        # Note, TaskItem, ChecklistItem, base entities
    ├── HobbyApp.Application/    # INoteService, ITaskService, IIdentityService, DTOs
    ├── HobbyApp.Infrastructure/ # ApplicationDbContext, Identity, migrations
    └── HobbyApp.Api/           # Controllers, Program.cs, appsettings
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for PostgreSQL) — or a local PostgreSQL instance
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

## Getting started

```bash
# 1. Start PostgreSQL
docker compose up -d

# 2. Apply database migrations
dotnet ef database update -p src/HobbyApp.Infrastructure -s src/HobbyApp.Api

# 3. Run the API
dotnet run --project src/HobbyApp.Api
```

Dev endpoints (see `src/HobbyApp.Api/Properties/launchSettings.json`):
- HTTP: `http://localhost:5169`
- HTTPS: `https://localhost:7058`
- OpenAPI document: `/openapi/v1.json` (Development only)

> In **Development** the API binds to `0.0.0.0` and skips HTTPS redirection, so a
> physical device on the same network can reach it over HTTP at
> `http://<your-LAN-IP>:5169`.

## Configuration

Settings live in `src/HobbyApp.Api/appsettings.json` (local dev values).
Override per environment with environment variables (double underscore = nesting):

| Setting | Env var | Purpose |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | PostgreSQL connection |
| `JwtSettings:Key` | `JwtSettings__Key` | JWT signing key (≥ 32 chars) |
| `JwtSettings:Issuer` / `Audience` | `JwtSettings__Issuer` / `__Audience` | Token issuer/audience |
| `JwtSettings:AccessTokenExpirationMinutes` | … | Access token lifetime |
| `JwtSettings:RefreshTokenExpirationDays` | … | Refresh token lifetime |

> **Production:** supply a strong, unique `JwtSettings:Key` and DB credentials via
> environment variables or a secret store — do not reuse the local dev values.

## API reference

All `/api/notes` and `/api/tasks` endpoints require a `Bearer` access token and
operate only on the authenticated user's data.

### Auth — `/api/auth`
| Method | Route | Description |
|---|---|---|
| POST | `/register` | Create account, returns tokens |
| POST | `/login` | Authenticate, returns tokens |
| POST | `/refresh` | Exchange a refresh token for a new token pair |
| POST | `/logout` | Revoke a refresh token |
| GET | `/me` | Current user's id + email |

### Notes — `/api/notes`
| Method | Route | Description |
|---|---|---|
| GET | `/?view=Active\|Archived\|Trash&search=` | List notes |
| GET | `/{id}` | Get a note |
| POST | `/` | Create a note |
| PUT | `/{id}` | Update title/content/color |
| PUT | `/{id}/pinned` | Set pinned (body: `true`/`false`) |
| PUT | `/{id}/archived` | Set archived (body: `true`/`false`) |
| POST | `/{id}/trash` | Move to trash (soft delete) |
| POST | `/{id}/restore` | Restore from trash |
| DELETE | `/{id}` | Delete permanently |

A note's `content` is a JSON array of blocks (typed-text and handwriting/SVG-stroke
blocks); the server stores it as `jsonb` and treats it as opaque.

### Tasks — `/api/tasks`
| Method | Route | Description |
|---|---|---|
| GET | `/?search=` | List tasks |
| GET | `/{id}` | Get a task |
| POST | `/` | Create a task with checklist items |
| PUT | `/{id}` | Update title/items |
| DELETE | `/{id}` | Delete a task |

## Database migrations

```bash
# Add a migration
dotnet ef migrations add <Name> -p src/HobbyApp.Infrastructure -s src/HobbyApp.Api -o Persistence/Migrations

# Apply migrations
dotnet ef database update -p src/HobbyApp.Infrastructure -s src/HobbyApp.Api
```

## Docker

The `Dockerfile` builds and publishes the API (build context = repo root):

```bash
docker build -t hobbyapp-api .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=...;Database=hobbyapp;Username=...;Password=..." \
  -e JwtSettings__Key="<strong-secret>" \
  hobbyapp-api
```
