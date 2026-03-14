# EM2Devs Building Management

[![CI](https://github.com/yurieastwood/em2devs-apps-building-management/actions/workflows/ci.yaml/badge.svg)](https://github.com/yurieastwood/em2devs-apps-building-management/actions/workflows/ci.yaml)
[![ADRs Site](https://github.com/yurieastwood/em2devs-apps-building-management/actions/workflows/log4brains.yaml/badge.svg)](https://github.com/yurieastwood/em2devs-apps-building-management/actions/workflows/log4brains.yaml)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
[![License: AGPL-3.0](https://img.shields.io/badge/License-AGPL--3.0-blue.svg)](LICENSE)

A multi-tenant SaaS platform for residential and commercial building management, built with .NET 10 and Clean Architecture.

## Overview

This system provides building managers (property administrators) with tools to manage buildings, units, residents, announcements, documents, scheduled visits, and incident tracking. It considers data privacy compliance.

### Bounded Contexts

| Context | Description |
|---------|-------------|
| **Authentication** | Registration, login, JWT token refresh, logout |
| **Managers** | Manager CRUD, building assignment, deactivation |
| **Buildings** | Building and unit CRUD, deactivation |
| **Residents** | Resident registration, updates, move-out, data privacy compliance |
| **Announcements** | Create, publish, archive announcements with audience targeting |
| **Documents** | Upload, share, and manage access to documents |
| **Visits** | Schedule inspections, checklists, follow-up actions |
| **Incidents** | Report, track, and resolve maintenance/security incidents |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (preview)
- [Docker](https://www.docker.com/get-started) (for containerized runs)

## Project Structure

```
em2devs-apps-building-management/
├── src/
│   ├── EM2Devs.BuildingManagement.Domain/           # Entities, value objects, domain events
│   ├── EM2Devs.BuildingManagement.Application/       # Use cases, contracts (DTOs), interfaces
│   ├── EM2Devs.BuildingManagement.Infrastructure/    # Persistence, external services
│   └── EM2Devs.BuildingManagement.Api/               # Minimal API endpoints, Program.cs
├── tests/
│   └── EM2Devs.BuildingManagement.Api.Unit.Tests/    # API integration/unit tests
├── docs/
│   ├── adr/                                          # Architecture Decision Records
│   └── domain-model.md                               # Domain model documentation
├── .husky/                                           # Git hooks (Husky.NET)
├── Dockerfile
└── em2devs-apps-building-management.sln
```

The solution follows **Clean Architecture** (see [ADR-0001](docs/adr/20260224-use-clean-architecture.md)):
- **Domain** — No external dependencies. Contains entities, value objects, and domain events.
- **Application** — References Domain only. Contains use-case interfaces and request/response contracts.
- **Infrastructure** — Implements Application interfaces. Persistence (EF Core + PostgreSQL), file storage, etc.
- **Api** — ASP.NET Core Minimal API entry point. Maps HTTP endpoints to application contracts.

## Running Locally

```bash
# Restore and build
dotnet build

# Run the API (Development mode)
dotnet run --project src/EM2Devs.BuildingManagement.Api

# The API will be available at:
#   http://localhost:5286
#   https://localhost:7035
```

The OpenAPI spec is served at `/openapi/v1.json` in Development mode.

## Running with Docker

### Build the image

```bash
docker build -t em2devs-building-management .
```

### Run the container

**Development** (enables OpenAPI/Scalar UI, uses built-in dev JWT defaults):

```bash
docker run -d -p 8080:8080 --name building-management \
  -e ASPNETCORE_ENVIRONMENT=Development \
  em2devs-building-management
```

**Production** (requires explicit JWT configuration):

```bash
docker run -d -p 8080:8080 --name building-management \
  -e Jwt__Key="<your-secret-key-min-32-chars>" \
  -e Jwt__Issuer="https://your-issuer" \
  -e Jwt__Audience="your-audience" \
  em2devs-building-management
```

> **Note:** Use double underscores (`__`) as the section separator for ASP.NET Core environment variable configuration. The application will fail fast on startup if any JWT setting is missing in non-Development environments.

The API will be available at `http://localhost:8080`.

### Build and run tests inside Docker

The Dockerfile includes a `test` stage. To run tests during the build:

```bash
docker build --target test -t em2devs-building-management-test .
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage report (Cobertura format)
dotnet test -c Release /p:CollectCoverage=true

# Run tests with verbose output
dotnet test --verbosity normal
```

The test project enforces an **80% coverage threshold** (line, branch, and method) via Coverlet. Coverage reports are output to `tests/TestResults/coverage.cobertura.xml`.

### Mutation Testing

[Stryker.NET](https://github.com/stryker-mutator/stryker-net) validates that the test suite catches real code changes ([ADR](docs/adr/20260313-use-stryker-net-for-mutation-testing.md)):

```bash
# Full run
STRYKER_MUTATING=true dotnet stryker

# Incremental (changed code only — used by pre-push hook and CI)
STRYKER_MUTATING=true dotnet stryker --since:main
```

HTML reports are generated in `StrykerOutput/`. Configuration lives in `stryker-config.json`.

## API Endpoints

The API exposes 53 endpoints across 8 groups. All endpoints except `/auth/*` require authorization.

| Group | Base Path | Endpoints |
|-------|-----------|-----------|
| Authentication | `/auth` | `POST register`, `login`, `refresh`, `logout` |
| Managers | `/managers` | `POST`, `GET /{id}`, `PATCH assign/revoke/deactivate` |
| Buildings | `/buildings` | `POST`, `GET`, `GET /{id}`, `PUT /{id}`, units CRUD, deactivate |
| Residents | `/residents` | `POST`, `GET`, `GET /{id}`, `PATCH /{id}`, invite, move-out, data privacy compliance |
| Announcements | `/announcements` | `POST`, `GET`, `GET /{id}`, `PUT /{id}`, publish, archive, delete |
| Documents | `/documents` | `POST /upload`, `GET`, `GET /{id}`, share, revoke access, delete |
| Visits | `/visits` | `POST`, `GET`, `GET /{id}`, start, complete, checklist items, follow-ups |
| Incidents | `/incidents` | `POST`, `GET`, `GET /{id}`, update status, resolution notes |

An interactive API reference powered by [Scalar](https://scalar.com/) is available in Development mode at `/scalar/v1`. The raw OpenAPI spec is served at `/openapi/v1.json`.

Sample HTTP requests are also available in [`src/EM2Devs.BuildingManagement.Api/EM2Devs.BuildingManagement.Api.http`](src/EM2Devs.BuildingManagement.Api/EM2Devs.BuildingManagement.Api.http) for use with VS Code REST Client or JetBrains HTTP Client.

## Development Workflow

### Git Hooks (Husky.NET)

The project uses [Husky.NET](https://alirezanet.github.io/Husky.Net/) for automated quality gates:

| Hook | Tasks |
|------|-------|
| **pre-commit** | `dotnet format --verify-no-changes`, `dotnet build -c Release`, `dotnet test -c Release` |
| **commit-msg** | Conventional Commits validation (e.g., `feat: ...`, `fix(auth): ...`) |
| **pre-push** | Branch name validation, mutation testing (`dotnet stryker --since:main`) |

### Commit Message Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>[optional scope]: <description>

Valid types: feat, fix, build, chore, ci, docs, style, refactor, perf, test

Examples:
  feat: add user registration
  fix(auth): resolve token expiration bug
  docs: update domain model
```

### Branch Naming

Follow [Conventional Branches](https://conventional-branch.github.io/#summary):

```
<type>/<scope>/<short-description>

Valid types: feat, bugfix, hotfix, release, chore

Examples:
  feat/add-login-endpoint
  bugfix/fix-move-out-date-validation
  release/v1.0.0
```

## Architecture Decision Records

Key architectural decisions are documented in [`docs/adr/`](docs/adr/).

## Tech Stack

- **.NET 10** (preview) with Minimal APIs
- **ASP.NET Core OpenAPI** for automatic spec generation
- **Entity Framework Core** with PostgreSQL (planned)
- **xUnit** + **Coverlet** for testing and coverage, **Stryker.NET** for mutation testing
- **Husky.NET** for git hooks
- **Docker** for containerization

## License

This project is licensed under the [GNU Affero General Public License v3.0](LICENSE).
