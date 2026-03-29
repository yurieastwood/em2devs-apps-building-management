# Building Management Platform — Project Roadmap

**Version:** 1.0
**Date:** 2026-03-29
**Status:** Draft
**Scope:** MVP (Phase 1 — Manager-only API)

---

## Context

This is a .NET 10 multi-tenant SaaS platform for residential/commercial building management. The **API surface is fully defined** (53 endpoints across 8 bounded contexts) with comprehensive DTOs, but **all endpoints return stub data**. No domain logic, persistence, or business rules exist yet. The project has strong engineering foundations (CI/CD, mutation testing, git hooks, ADRs, Docker) and a detailed [domain model specification](domain-model.md).

**Goal:** Implement the MVP end-to-end, turning the stub API into a fully functional backend with PostgreSQL persistence, enforced business rules, and LGPD compliance.

### What's Done

- Clean Architecture project structure (Domain, Application, Infrastructure, API)
- 53 API endpoint definitions with proper routing (all return stub/mock data)
- Application layer DTOs (`sealed record`) for all bounded contexts
- JWT Bearer authentication configuration
- OpenAPI / Scalar interactive API docs
- Test infrastructure: xUnit, Coverlet (80% threshold), Stryker.NET mutation testing
- CI/CD pipeline (GitHub Actions): lint, format, build, test, mutation
- Git hooks (Husky.NET): pre-commit, pre-push, commit-msg (conventional commits)
- Docker multi-stage build
- 10 Architecture Decision Records
- Detailed domain model document with aggregates, entities, value objects, commands, events, workflows

### What's Not Done

- Domain layer entities, value objects, and aggregates
- Application layer use cases and services
- Infrastructure layer (EF Core DbContext, repositories, PostgreSQL schema)
- Business logic (all endpoints return stubs)
- Domain event publishing (Outbox Pattern)
- LGPD erasure background service
- File storage implementation
- Integration tests for tenant isolation
- Domain unit tests for invariants

---

## Phase 1: Domain Layer Foundation

**Objective:** Implement all domain entities, value objects, and aggregate roots with enforced invariants. Pure C# with no infrastructure dependencies.

### 1A — Shared Domain Kernel & Upstream Contexts (IAM + Building Management)

These are upstream of all other bounded contexts per the [context map](domain-model.md#3-context-map).

**Tasks:**
- Create base types: `Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`, `IDomainEvent`
- Create strongly-typed IDs: `TenantId`, `ManagerId`, `BuildingId`, `UnitId` (GUIDs)
- Implement **Tenant** aggregate (Name uniqueness, Active/Suspended status)
- Implement **Manager** aggregate (Email unique within Tenant, `ManagerRole` VO, `AssignedBuildings` VO, soft delete)
- Implement **Building** aggregate (`Address` VO, soft delete, cascade deactivation with `force` flag)
- Implement **Unit** entity (`UnitType` VO, UnitNumber unique within Building, soft delete)
- Implement all status transition state machines

**Testing:**
- Unit tests for every invariant: Manager email uniqueness, Building cascade deactivation, Unit uniqueness within Building
- Unit tests for all value object equality
- Unit tests for AssignBuildingToManager / RevokeBuildingFromManager

### 1B — Downstream Contexts (Resident, Announcement, Document, Visit, Incident)

**Tasks:**
- Implement **Resident** aggregate (`ResidentRole` VO, `ResidentStatus` VO, `OccupancyPeriod` VO, PII field marking, one Owner + one Renter per Unit invariant, soft delete)
- Implement **Announcement** aggregate (`AnnouncementStatus` VO with Draft -> Published -> Archived, `AudienceSpecification` VO with scope validation)
- Implement **Document** aggregate (`DocumentAccessGrant` entity, `StorageReference` VO, `GranteeType` VO, idempotent grant/revoke)
- Implement **Visit** aggregate (`VisitStatus` VO, `ChecklistItem` entity, `FollowUpAction` entity, `ChecklistItemStatus` VO, `ChecklistCategory` VO, all-items-resolved-before-complete invariant)
- Implement **Incident** aggregate (`IncidentType` VO, `IncidentSeverity` VO, `IncidentStatus` VO with Open -> InProgress -> Resolved, `IncidentLocation` VO, immutable when Resolved)

**Testing:**
- Unit tests for all status transitions (Announcement lifecycle, Visit completion gate, Incident immutability)
- Unit tests for `AudienceSpecification` scope validation
- Unit tests for Resident role constraints (one Owner, one Renter per Unit)
- Unit tests for Document access grant idempotency

### 1C — Domain Events

**Tasks:**
- Define all domain event types from the [domain model](domain-model.md#6-domain-events): `TenantProvisioned`, `ManagerCreated`, `BuildingCreated`, `UnitAdded`, `ResidentRegistered`, `AnnouncementPublished`, `IncidentReported`, `VisitCompleted`, etc.
- Wire event raising into aggregate root methods (collected, not dispatched — dispatching is infrastructure)

**Testing:**
- Verify aggregates raise correct events on state changes

---

## Phase 2: Application Layer

**Objective:** Define use case interfaces, command/query handlers, and application services that orchestrate domain logic.

### 2A — Interfaces & Abstractions

**Tasks:**
- Define repository interfaces per aggregate: `ITenantRepository`, `IManagerRepository`, `IBuildingRepository`, `IResidentRepository`, `IAnnouncementRepository`, `IDocumentRepository`, `IVisitRepository`, `IIncidentRepository`
- Define `IUnitOfWork` interface (transactional consistency)
- Define `IFileStorageService` interface (per [ADR-0004](adr/20260224-use-storage-abstraction-for-file-management.md): `SaveAsync`, `GetAsync`, `DeleteAsync` with tenant-scoped paths)
- Define `ICurrentTenantProvider` interface (extracts TenantId from JWT claims)
- Define `IAudienceResolver` domain service interface (resolves `AudienceSpecification` to ResidentIds)

### 2B — Command & Query Handlers (Use Cases)

**Tasks:**
- Implement command handlers for each bounded context, mapping from DTOs to domain operations:
  - **IAM:** CreateManager, AssignBuilding, RevokeBuilding, DeactivateManager
  - **Buildings:** CreateBuilding, UpdateBuilding, AddUnit, UpdateUnit, DeactivateBuilding
  - **Residents:** RegisterResident, InviteResident, UpdateResidentInfo, MoveOutResident
  - **Announcements:** CreateAnnouncement, UpdateAnnouncement, PublishAnnouncement, ArchiveAnnouncement, DeleteDraft
  - **Documents:** UploadDocument, ShareDocument, RevokeAccess, DeleteDocument
  - **Visits:** ScheduleVisit, StartVisit, AddChecklistItem, ResolveChecklistItem, AddFollowUpAction, LinkToIncident, CompleteFollowUpAction, CompleteVisit
  - **Incidents:** ReportIncident, UpdateStatus, AddResolutionNotes
- Implement query handlers for all GET/list endpoints with pagination support
- Implement `AudienceResolver` domain service

**Testing:**
- Unit tests for command handlers with mocked repositories
- Verify proper TenantId scoping in all handlers
- Verify cross-context queries (e.g., Building validity check during ScheduleVisit)

---

## Phase 3: Infrastructure Layer

**Objective:** Implement EF Core persistence with PostgreSQL, multi-tenancy enforcement, and supporting services.

### 3A — EF Core DbContext & Entity Configuration

**Tasks:**
- Create `BuildingManagementDbContext` with EF Core
- Configure entity mappings for all aggregates (Fluent API)
- Add global query filters for TenantId isolation (per [ADR-0003](adr/20260224-use-ef-core-with-postgresql-for-persistence.md))
- Add global query filter for soft delete (`DeletedAt == null`)
- Add `SaveChanges` interceptor to enforce TenantId on all writes
- Create initial migration with TenantId indexes on all aggregate tables
- Configure value object mapping (owned entities or value converters)
- Update `compose.yaml` to add PostgreSQL service

### 3B — Repository Implementations

**Tasks:**
- Implement all repository interfaces from Phase 2A
- Implement `IUnitOfWork` wrapping `DbContext.SaveChangesAsync()`
- Ensure all queries are TenantId-scoped (enforced by global filters)
- Implement pagination support for list queries

### 3C — Outbox Pattern for Domain Events (per [ADR-0007](adr/20260303-use-outbox-pattern-for-domain-events.md))

**Tasks:**
- Create `OutboxMessage` table (Id, EventType, Payload, TenantId, OccurredAt, ProcessedAt)
- Implement `SaveChanges` interceptor that persists domain events to outbox in same transaction
- Implement background dispatcher (hosted service) that processes unprocessed outbox messages
- 90-day retention for processed messages (audit trail)

### 3D — File Storage (per [ADR-0004](adr/20260224-use-storage-abstraction-for-file-management.md))

**Tasks:**
- Implement `LocalFileStorageService` (MVP — stores files on disk, tenant-scoped directories)
- Register as `IFileStorageService` implementation
- Wire into Document upload/download endpoints

### 3E — Authentication Infrastructure

**Tasks:**
- Implement JWT token generation service (for login/register endpoints)
- Implement refresh token storage and rotation
- Implement password hashing (BCrypt or Argon2)
- Implement `ICurrentTenantProvider` from JWT claims

**Testing:**
- Integration tests for TenantId isolation (verify Tenant A cannot see Tenant B data)
- Integration tests for soft delete filtering
- Integration tests for outbox message persistence
- Integration tests for file storage operations

---

## Phase 4: Wire Endpoints to Real Logic

**Objective:** Replace all stub responses with actual command/query handler invocations.

### 4A — Dependency Injection Setup

**Tasks:**
- Register all services, repositories, and handlers in `Program.cs`
- Configure EF Core with PostgreSQL connection string
- Configure file storage provider
- Add health checks for database connectivity

### 4B — Replace Stubs per Bounded Context

**Tasks (in dependency order):**
1. **Authentication** endpoints -> JWT service, Manager creation
2. **Manager** endpoints -> ManagerRepository, command handlers
3. **Building** endpoints -> BuildingRepository, command handlers
4. **Resident** endpoints -> ResidentRepository, command handlers
5. **Announcement** endpoints -> AnnouncementRepository, AudienceResolver, command handlers
6. **Document** endpoints -> DocumentRepository, FileStorageService, command handlers
7. **Visit** endpoints -> VisitRepository, command handlers
8. **Incident** endpoints -> IncidentRepository, command handlers

### 4C — Error Handling & Validation Middleware

**Tasks:**
- Map domain exceptions to HTTP status codes (422 for invariant violations, 404 for not found, 409 for conflicts)
- Use the existing `ErrorResponse` DTO for consistent error format
- Add request validation (FluentValidation or manual) at the endpoint level
- Add global exception handler middleware

**Testing:**
- Update existing `EndpointRouteTests` to verify real responses instead of stubs
- Add end-to-end tests for critical workflows (W-01 through W-05 from domain model)
- Verify OpenAPI spec still valid with real implementations

---

## Phase 5: LGPD Compliance & Cross-Cutting

**Objective:** Implement data privacy compliance and remaining cross-cutting concerns.

### 5A — LGPD Erasure Background Service (per [ADR-0008](adr/20260303-lgpd-data-erasure-strategy.md))

**Tasks:**
- Implement daily `BackgroundService` running at 02:00 UTC
- Query Residents with MoveOutDate > 5 years ago and non-pseudonymized PII
- Block erasure if open/in-progress Incidents reference the Resident
- Pseudonymize PII fields with `[ERASED-{hash}]` tokens
- Emit `ResidentPIIErased` domain event
- Implement `DELETE /residents/{residentId}/personal-data` endpoint (manual erasure trigger)
- Make idempotent

### 5B — Correlation & Observability

**Tasks:**
- Add `CorrelationId` header propagation (trace ID on all commands/events)
- Add structured logging for command execution and domain events
- Ensure PII fields are never logged

**Testing:**
- Integration test: LGPD erasure after 5-year retention
- Integration test: erasure blocked by open Incident
- Integration test: pseudonymization preserves referential integrity
- Verify PII not present in logs

---

## Phase 6: Production Readiness

**Objective:** Harden for deployment.

**Tasks:**
- Add rate limiting middleware
- Add request size limits (especially for document upload)
- Configure CORS
- Add database migration strategy (EF Core migrations in CI/CD)
- Update Docker compose with PostgreSQL service and health checks
- Add API versioning strategy
- Security audit: validate all endpoints require authorization (except `/auth`)
- Load test critical endpoints
- Update README with deployment instructions

---

## Verification Plan

After each phase, verify:
1. `dotnet build -c Release` passes
2. `dotnet test -c Release /p:CollectCoverage=true` passes with >= 80% coverage
3. `dotnet stryker` mutation testing passes
4. `dotnet format --verify-no-changes` passes
5. Docker build succeeds: `docker build .`

End-to-end smoke test after Phase 4:
1. Register a Manager -> Login -> Get JWT
2. Create Building -> Add Unit
3. Register Resident -> Invite -> Move-out
4. Create Announcement -> Publish -> Archive
5. Upload Document -> Share -> Revoke
6. Schedule Visit -> Start -> Add checklist -> Complete
7. Report Incident -> Update status -> Resolve
8. Verify tenant isolation: repeat with a second tenant, confirm data separation

---

## Dependency Graph

```
Phase 1A (Domain: IAM + Buildings)
    |
Phase 1B (Domain: Downstream contexts) -- depends on 1A for shared types + upstream IDs
    |
Phase 1C (Domain Events) -- depends on 1A + 1B for aggregate methods
    |
Phase 2A (Application: Interfaces) -- depends on Phase 1 for domain types
    |
Phase 2B (Application: Handlers) -- depends on 2A for interfaces
    |
Phase 3A-3E (Infrastructure) -- depends on Phase 2 for interfaces to implement
    |
Phase 4A-4C (Wire Endpoints) -- depends on Phase 3 for real implementations
    |
Phase 5 (LGPD + Cross-cutting) -- depends on Phase 4 for working system
    |
Phase 6 (Production Readiness) -- depends on Phase 5
```
