# Building Management Platform — Agent Development Guide

This document provides context for AI agents working on the EM2Devs Building Management codebase.
For project overview, setup instructions, and API surface see [`README.md`](README.md).

## Architecture

### Clean Architecture Layers

The solution enforces strict dependency rules ([ADR-0001](docs/adr/20260224-use-clean-architecture.md)):

```
API  →  Infrastructure  →  Application  →  Domain
 └──────────────────────────→ Application  →  Domain
```

| Layer | Project | May Reference | Responsibility |
|-------|---------|---------------|----------------|
| **Domain** | `EM2Devs.BuildingManagement.Domain` | Nothing | Entities, value objects, domain events, invariants. Zero external dependencies. |
| **Application** | `EM2Devs.BuildingManagement.Application` | Domain | Use-case interfaces, request/response contracts (DTOs), validation rules. |
| **Infrastructure** | `EM2Devs.BuildingManagement.Infrastructure` | Application, Domain | EF Core + PostgreSQL persistence ([ADR-0003](docs/adr/20260224-use-ef-core-with-postgresql-for-persistence.md)), file storage ([ADR-0004](docs/adr/20260224-use-storage-abstraction-for-file-management.md)), external services. |
| **Api** | `EM2Devs.BuildingManagement.Api` | Application, Infrastructure | ASP.NET Core Minimal API entry point. Endpoint mapping, middleware pipeline, DI wiring. |

**Never** add a dependency that violates this direction (e.g., Domain referencing Infrastructure).

### Key Architectural Decisions

All ADRs live in [`docs/adr/`](docs/adr/) and are managed with log4brains ([ADR-0005](docs/adr/20260226-use-log4brains-to-manage-the-adrs.md)).

Before making an architectural decision that contradicts or extends an existing ADR, document a new ADR.

## Domain Model

The authoritative domain specification is [`docs/domain-model.md`](docs/domain-model.md). Key points for agents:

### Ubiquitous Language

Use the canonical terms defined in the glossary. Common traps:

| Use This | NOT This |
|----------|----------|
| **Tenant** | client, organization, company |
| **Building** | community (synonym only — not a sub-level), property, complex |
| **Unit** | apartment, suite, room |
| **Resident** | tenant (conflicts with SaaS Tenant), occupant |
| **Manager** | admin, landlord |
| **Visit** | inspection, walkthrough |
| **Incident** | issue, ticket |

### Multi-Tenancy

All aggregate roots carry a `TenantId` discriminator. Every query and command must be scoped to the current tenant. Never expose cross-tenant data.

### LGPD / Data Privacy ([ADR-0008](docs/adr/20260303-lgpd-data-erasure-strategy.md))

- PII fields live exclusively in the Resident Registry context (CPF, name, email, phone).
- Erasure means pseudonymization (`[ERASED-{hash}]`), not physical deletion.
- 5-year retention after `MoveOutDate` before eligibility.
- Erasure is blocked while the Resident has open Incidents.
- Background service runs daily at 02:00 UTC; process is idempotent.

### Domain Events ([ADR-0007](docs/adr/20260303-use-outbox-pattern-for-domain-events.md))

Domain events are persisted via the Outbox Pattern — they are written transactionally alongside aggregate changes and dispatched asynchronously. Key events: `ResidentMovedOut`, `ResidentPIIErased`.

## Code Patterns & Conventions

### Endpoint Files

Each bounded context has a single static class in `src/EM2Devs.BuildingManagement.Api/Endpoints/`:

```
*Endpoints.cs  →  static class with Map*Endpoints() extension method on IEndpointRouteBuilder
```

Pattern:
```csharp
public static class FooEndpoints
{
    public static IEndpointRouteBuilder MapFooEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/foos")
            .WithTags("Foos")
            .RequireAuthorization();

        group.MapPost("/", (...) => { ... });
        // ...
        return app;
    }
}
```

All endpoint groups except Authentication require authorization via `.RequireAuthorization()`.

### DTO Contracts

DTOs are **sealed records** in `src/EM2Devs.BuildingManagement.Application/Contracts/`, organized by bounded context:

```
Contracts/
├── Announcements/    # CreateAnnouncementRequest, AnnouncementResponse, ...
├── Authentication/   # RegisterRequest, LoginRequest, AuthResponse, ...
├── Buildings/        # CreateBuildingRequest, BuildingResponse, AddressDto, ...
├── Common/           # ErrorResponse, PagedResponse<T>, PaginationRequest
├── Documents/        # UploadDocumentRequest, DocumentResponse, ...
├── Incidents/        # ReportIncidentRequest, IncidentResponse, ...
├── Managers/         # CreateManagerRequest, ManagerResponse
├── Residents/        # RegisterResidentRequest, ResidentResponse, ...
└── Visits/           # ScheduleVisitRequest, VisitResponse, ChecklistItemResponse, ...
```

Naming:
- Inbound: `*Request` (e.g., `CreateBuildingRequest`)
- Outbound: `*Response` (e.g., `BuildingResponse`)
- Nested/shared: `*Dto` (e.g., `AddressDto`, `AudienceSpecificationDto`)

All DTOs are `public sealed record` types. Use `init`-only properties.

### C# Style

- **Nullable** reference types enabled (`<Nullable>enable</Nullable>`)
- **Implicit usings** enabled
- **Top-level statements** in `Program.cs`
- Use `dotnet format` style — the pre-commit hook enforces it
- Prefer file-scoped namespaces (`namespace Foo;`)
- GUIDs for all entity identifiers

## Common Tasks

### Adding a New Endpoint

1. If needed, create request/response DTOs as `sealed record` in `Application/Contracts/<Context>/`.
2. Open or create the `*Endpoints.cs` file in `Api/Endpoints/`.
3. Add the route inside the existing `MapGroup()`, following existing patterns.
4. Register the endpoint group in `Program.cs` if it's a new file: `app.Map*Endpoints();`.
5. Add test coverage in the corresponding test class.

### Adding a New DTO

1. Create a `public sealed record` in `Application/Contracts/<Context>/`.
2. DTOs are excluded from coverage analysis — no tests required for the type itself.
3. If it's a request type with validation logic, test that logic.

### Adding a New Bounded Context

1. Create a subfolder under `Application/Contracts/` for the DTOs.
2. Create an `*Endpoints.cs` in `Api/Endpoints/`.
3. Register with `app.Map*Endpoints()` in `Program.cs`.
4. Add corresponding endpoint tests.
5. Update the domain model documentation in `docs/domain-model.md`.

### Adding an Architecture Decision Record

Use log4brains:

```bash
npx log4brains adr new
```

Or manually create a markdown file in `docs/adr/` following `docs/adr/template.md`.

## Testing

### Structure

Tests live in `tests/EM2Devs.BuildingManagement.Api.Unit.Tests/`:

| File | Purpose |
|------|---------|
| `TestWebApplicationFactory.cs` | Custom `WebApplicationFactory<Program>` with test auth handler |
| `EndpointRouteTests.cs` | Verifies all 53 endpoints return expected status codes |
| `OpenApiSpecTests.cs` | Validates OpenAPI spec generation |
| `PagedResponseTests.cs` | Paging response contract validation |
| `PaginationRequestTests.cs` | Pagination request contract validation |

### Rules

- **80% coverage threshold** enforced (line, branch, method) via Coverlet.
- Coverage excludes: `Program.cs`, all DTO contracts, `ErrorResponse`, `obj/` directories.
- Tests use `IClassFixture<TestWebApplicationFactory>` for shared test server.
- Test auth uses fixed GUIDs for user and tenant claims (`TestAuthHandler`).
- Endpoint tests use typed contract DTOs, not anonymous objects.
- **Do not** duplicate production logic in test helpers — call production code directly.

### Running Tests

```bash
dotnet test                                          # All tests
dotnet test -c Release /p:CollectCoverage=true       # With coverage
```

### Mutation Testing ([ADR-0010](docs/adr/20260313-use-stryker-net-for-mutation-testing.md))

[Stryker.NET](https://github.com/stryker-mutator/stryker-net) validates that the test suite catches real code changes. Config lives in `stryker-config.json`.

```bash
dotnet stryker                                       # Full run
dotnet stryker --since:main                          # Incremental (changed code only)
```

- **Break threshold**: 50% — builds fail below this score.
- **Mutated sources**: all `src/**/*.cs` except `Program.cs`, DTO contracts, and migrations.
- Reports are generated in `StrykerOutput/` (git-ignored).
- Pre-push hook runs incremental mode (`--since:main`) automatically.
- CI runs incremental mode against the PR base branch and uploads the HTML report as an artifact.

## Quality Gates

### Pre-Commit (Husky.NET)

Every commit automatically runs:
1. `dotnet format --verify-no-changes` — code style
2. `dotnet build -c Release` — compilation
3. `dotnet test -c Release` — full test suite

If any step fails, the commit is rejected. **Do not bypass hooks** with `--no-verify`.

### Commit Messages

Conventional Commits format is enforced by the `commit-msg` hook:

```
<type>[optional scope]: <description>

Valid types: feat, fix, build, chore, ci, docs, style, refactor, perf, test
```

### Pre-Push (Husky.NET)

Every push automatically runs:
1. Branch name validation (Conventional Branches)
2. `dotnet stryker --since:main` — mutation testing (incremental)

If any step fails, the push is rejected.

### Branch Names

Validated on push by the `pre-push` hook:

```
<type>/<description>

Valid types: feat, fix, bugfix, hotfix, release, chore
```

### CI Pipeline

GitHub Actions (`.github/workflows/ci.yaml`) runs: lint-commit, lint-branch, lint-format, build, test, mutation. All checks must pass before merge.

## Project Entry Points

| What | Where |
|------|-------|
| Solution file | `em2devs-apps-building-management.sln` |
| App entry point | `src/EM2Devs.BuildingManagement.Api/Program.cs` |
| Endpoint registration | `Program.cs` lines 48–55 (8 `Map*Endpoints()` calls) |
| All endpoints | `src/EM2Devs.BuildingManagement.Api/Endpoints/` |
| All contracts/DTOs | `src/EM2Devs.BuildingManagement.Application/Contracts/` |
| Domain model spec | `docs/domain-model.md` |
| ADRs | `docs/adr/` |
| Test project | `tests/EM2Devs.BuildingManagement.Api.Unit.Tests/` |
| CI pipeline | `.github/workflows/ci.yaml` |
| Docker config | `Dockerfile`, `compose.yaml` |
| Git hooks | `.husky/task-runner.json` |
| Mutation config | `stryker-config.json` |
| HTTP samples | `src/EM2Devs.BuildingManagement.Api/EM2Devs.BuildingManagement.Api.http` |

## Things to Watch Out For

- **Layer violations** — never add a project reference that breaks the Clean Architecture dependency flow.
- **Tenant scoping** — every data access path must filter by `TenantId`. Missing this leaks data across tenants.
- **Ubiquitous language drift** — use the canonical terms from the glossary. Code, API paths, and DTOs must match.
- **LGPD** — any new PII field must be added to the erasure pipeline. Don't store PII outside the Resident context.
- **Sealed records** — all DTOs must be `sealed record`. Don't use classes or unsealed records.
- **Auth on endpoints** — every endpoint group except `/auth` must call `.RequireAuthorization()`.
- **Hook bypass** — never use `--no-verify`. If hooks fail, fix the underlying issue.
- **Coverage regression** — keep coverage above 80%. New code should be tested.
- **Mutation score** — keep mutation score above the break threshold (50%). Surviving mutants indicate weak assertions that should be strengthened.
