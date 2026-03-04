# LGPD Data Erasure Strategy

- Status: accepted
- Deciders: Development team
- Date: 2026-03-03
- Tags: compliance, lgpd, infrastructure

Technical Story: The domain model (Section 8.2) specifies PII retention and erasure rules for LGPD compliance. Resident PII must be pseudonymized automatically after a 5-year retention window, with safeguards against premature erasure when open Incidents exist.

## Context and Problem Statement

The Building Management system stores PII (Personally Identifiable Information) for Managers (Email, FullName) and Residents (FullName, Email, Phone, DocumentNumber). Under LGPD (Lei Geral de Proteção de Dados — Brazil's data protection law), data subjects have the right to erasure (Article 18, VI). The domain model specifies:

- PII is retained for 5 years after `MoveOutDate` (resolved hypothesis A2)
- Erasure is blocked while any open or in-progress Incident references the Resident (resolved hypothesis A3)
- Pseudonymization, not physical deletion — the row is preserved for audit trail
- A `ResidentPIIErased` event must be emitted for compliance logging

How should the system automate PII erasure to satisfy LGPD requirements?

## Decision Drivers

- LGPD Article 18(VI): right to erasure of personal data
- 5-year retention period after MoveOutDate (legal safety for property management disputes)
- Erasure must be blocked while any open/in-progress Incident references the Resident
- Pseudonymization preserves audit trail and referential integrity
- The process must be idempotent — the job may run multiple times on the same record
- No external job scheduler for MVP — use ASP.NET Core hosted services
- `ResidentPIIErased` event must be persisted via the outbox for compliance audit (ADR-0007)

## Considered Options

- Scheduled BackgroundService with daily sweep
- Event-driven via outbox with delayed execution
- Hangfire recurring job
- Manual admin endpoint only

## Decision Outcome

Chosen option: "Scheduled BackgroundService with daily sweep", because it requires no additional infrastructure, handles the 5-year delay naturally by checking dates rather than timers, is simple to test, and runs within the existing ASP.NET Core process.

The event-driven approach was considered but rejected because the 5-year delay between `ResidentMovedOut` and erasure eligibility makes durable delayed message scheduling impractical — this would require a message broker feature not available in MVP.

### Eligibility Criteria

A Resident is eligible for PII erasure when ALL conditions are met:

1. `Status = MovedOut`
2. `MoveOutDate <= DateTime.UtcNow.AddYears(-5)` (retention window expired)
3. PII fields are not already pseudonymized (`FullName` does not match `[ERASED-*]` pattern)
4. No open or in-progress Incidents reference this `ResidentId`

### Pseudonymization Strategy

PII fields are replaced with deterministic pseudonym tokens:

```csharp
var hash = ComputeSha256Hash(resident.ResidentId.ToString())[..8];

FullName       → "[ERASED-{hash}]"
Email          → "erased-{hash}@erased.local"
Phone          → "[ERASED-{hash}]"
DocumentNumber → "[ERASED-{hash}]"
```

Design rationale:
- **Deterministic hash** (from ResidentId): if the same Resident appears in multiple references, the pseudonym is consistent
- **`[ERASED-*]` prefix**: immediately visible in queries that data has been erased
- **Email format preserved**: `erased-{hash}@erased.local` satisfies email format constraints without being a deliverable address
- **NOT NULL preserved**: pseudonym values maintain NOT NULL column constraints
- **Idempotency**: checking for `[ERASED-*]` prefix prevents re-processing

### Background Job Design

```
LgpdErasureBackgroundService : BackgroundService
├── Runs daily at 02:00 UTC (configurable)
├── Uses IServiceScopeFactory for per-batch DI scoping
├── Processes in batches of 100 eligible Residents
├── For each eligible Resident:
│   ├── Pseudonymize PII fields via domain method
│   ├── Raise ResidentPIIErased domain event
│   └── SaveChanges (event persisted to outbox atomically)
└── Wraps each batch in try/catch — errors log and continue
```

### Application Layer Contract

```csharp
public interface ILgpdErasureService
{
    Task<int> ProcessPendingErasuresAsync(int batchSize, CancellationToken ct);
    Task<ErasureEligibilityResult> CheckEligibilityAsync(Guid residentId, CancellationToken ct);
}

public record ErasureEligibilityResult(
    bool IsEligible,
    string? BlockReason,
    DateTime? EligibleAfter
);
```

### Blocked Erasures

When erasure is blocked by an open Incident:

1. The record is flagged as `ErasurePending` with blocking Incident IDs recorded
2. A `ResidentErasureBlocked` domain event is emitted (future: consumed by notification handler to alert the Manager)
3. The next daily sweep re-checks the record — once all blocking Incidents are resolved, erasure proceeds

### Cross-Cutting Considerations

- **Multi-tenancy**: The erasure job queries across all tenants using `.IgnoreQueryFilters()` to bypass the tenant global query filter, but emits events with the correct `TenantId` context for each Resident
- **File storage**: Document access grants to the erased Resident are revoked, but Documents themselves are NOT deleted (they may be shared with other Residents). Physical file deletion is a separate admin decision
- **Manager PII**: Manager PII erasure follows the same pattern but is triggered by Manager account deactivation + 5-year retention. Deferred to a future phase since Managers are active users in MVP
- **Manual erasure**: A future admin endpoint can call the same `ILgpdErasureService.CheckEligibilityAsync` and `ProcessPendingErasuresAsync` for immediate on-demand erasure requests

### Positive Consequences

- Fully automated LGPD compliance with zero manual intervention
- Pseudonymization preserves audit trail and referential integrity
- Incident-blocking rule prevents premature data loss
- Daily sweep is simple, testable, and requires no message broker
- `ResidentPIIErased` event provides compliance audit trail via the outbox (ADR-0007)
- Idempotent design allows safe re-runs

### Negative Consequences

- Daily sweep means erasure happens up to 24 hours after the 5-year window expires (acceptable for compliance — the legal requirement is "without undue delay")
- The `BackgroundService` runs in the API process — if the API is not running, erasure does not happen (acceptable for MVP; mitigated by health checks and uptime monitoring)
- No manual "erase now" endpoint in MVP — can be added later using the same `ILgpdErasureService`

## Pros and Cons of the Options

### Scheduled BackgroundService with daily sweep

- Good, because no additional infrastructure — runs in the existing ASP.NET Core host
- Good, because handles the 5-year delay naturally by checking dates
- Good, because simple to test (inject a mock clock, verify pseudonymization)
- Good, because idempotent and resilient to restarts
- Bad, because erasure latency is up to 24 hours after eligibility
- Bad, because tied to the API process lifecycle

### Event-driven via outbox with delayed execution

- Good, because event-driven architecture is consistent with the Outbox Pattern
- Good, because the `ResidentMovedOut` event naturally triggers the workflow
- Bad, because a 5-year delay requires durable delayed message scheduling — a message broker feature not available in MVP
- Bad, because an in-process timer spanning years is unreliable across deployments and restarts

### Hangfire recurring job

- Good, because Hangfire provides a dashboard for monitoring and retry management
- Good, because more resilient than `BackgroundService` — jobs survive process restarts via persistent storage
- Bad, because introduces a NuGet dependency and requires its own database schema
- Bad, because adds operational complexity for a single recurring job
- Bad, because Hangfire's persistent storage duplicates what the outbox already provides

### Manual admin endpoint only

- Good, because simplest implementation — no background processing
- Good, because Manager has full control over when erasure happens
- Bad, because LGPD requires automated processing — manual-only does not satisfy "without undue delay"
- Bad, because relies on human discipline to trigger erasure for every eligible Resident
- Bad, because does not scale with growing Resident count

## Links

- Depends on [Use EF Core with PostgreSQL for Persistence](20260224-use-ef-core-with-postgresql-for-persistence.md) — erasure queries use EF Core with `.IgnoreQueryFilters()`
- Depends on [Use Outbox Pattern for Domain Events](20260303-use-outbox-pattern-for-domain-events.md) — `ResidentPIIErased` and `ResidentErasureBlocked` events are persisted via the outbox
- Related to [Use Clean Architecture](20260224-use-clean-architecture.md) — `ILgpdErasureService` in Application layer, `LgpdErasureBackgroundService` in Infrastructure layer
