# Use Outbox Pattern for Domain Event Publication

- Status: accepted
- Deciders: Development team
- Date: 2026-03-03
- Tags: infrastructure, events, persistence

Technical Story: The domain model defines 22 domain events across 7 bounded contexts (Section 6). A reliable publication mechanism is needed that guarantees events are not lost when an aggregate is persisted, while requiring no additional infrastructure for MVP.

## Context and Problem Statement

When a command modifies an aggregate and produces domain events, the system must guarantee that those events are reliably persisted and dispatched — even if the application crashes between saving the aggregate and publishing the event. This is the classic "dual-write" problem: the aggregate state and its events must be committed atomically. Additionally, domain events serve as the audit trail for LGPD compliance, making reliable persistence a regulatory requirement. How should domain events be published in a single-deployment .NET 10 application backed by PostgreSQL?

## Decision Drivers

- At-least-once delivery guarantee: if `SaveChanges` succeeds, the events must eventually be delivered to handlers
- No distributed transaction coordinator — the system uses a single PostgreSQL database with no message broker for MVP
- Domain events must be persisted for audit and LGPD compliance (traceable data operations)
- The Domain layer must remain free of external NuGet dependencies (ADR-0001)
- The solution must support future migration to an external message broker without domain or application layer changes
- Single deployment for MVP — all 7 bounded contexts share one process

## Considered Options

- Outbox Pattern with EF Core (same-transaction write)
- In-memory dispatch only (fire-and-forget after SaveChanges)
- Dedicated Event Store (EventStoreDB or Marten)
- Change Data Capture (PostgreSQL logical replication / Debezium)

## Decision Outcome

Chosen option: "Outbox Pattern with EF Core (same-transaction write)", because it solves the dual-write problem using the existing PostgreSQL database and EF Core transaction, requires no additional infrastructure, and the outbox table doubles as both event dispatch queue and audit log.

### No MediatR or MassTransit

Neither MediatR nor MassTransit is used for MVP:

- **MediatR** would add a NuGet dependency to the Domain project, violating ADR-0001's zero-dependency rule. A simple `IDomainEventDispatcher` interface (~30 LOC) achieves the same in-process dispatch. If MediatR is desired later, the migration cost is trivial: implement `IDomainEventDispatcher` using `IPublisher.Publish()`.
- **MassTransit** is a distributed messaging framework designed for RabbitMQ/Azure Service Bus. Using its in-memory transport for in-process dispatch would be an oversized dependency for the current single-deployment architecture.

### Event Storage Strategy

The outbox table IS the event store. There is no separate event store.

- **Unprocessed messages** (`ProcessedAt IS NULL`) are the dispatch queue
- **Processed messages** (`ProcessedAt IS NOT NULL`) are the event log / audit trail
- Processed messages are retained for 90 days (configurable); a nightly cleanup job removes older entries
- For compliance queries ("show me all events for Resident X"), query the outbox table by `TenantId` and filter the JSONB `Payload` by relevant fields — PostgreSQL GIN indexes make this performant

If event sourcing is adopted in the future (unlikely for this CRUD-heavy domain), a dedicated EventStoreDB can be introduced without domain changes.

### Implementation Approach

**Domain layer (zero NuGet dependencies):**

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    Guid TenantId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}

public abstract record DomainEvent(Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

**Application layer:**

```csharp
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct);
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct);
}
```

**Infrastructure — Outbox table:**

```sql
CREATE TABLE outbox_messages (
    id              UUID PRIMARY KEY,
    tenant_id       UUID NOT NULL,
    event_type      VARCHAR(256) NOT NULL,
    payload         JSONB NOT NULL,
    occurred_at     TIMESTAMPTZ NOT NULL,
    processed_at    TIMESTAMPTZ NULL,
    retry_count     INT NOT NULL DEFAULT 0,
    error           TEXT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IX_OutboxMessages_Unprocessed
    ON outbox_messages (created_at)
    WHERE processed_at IS NULL;

CREATE INDEX IX_OutboxMessages_TenantId
    ON outbox_messages (tenant_id, created_at);
```

This table does NOT have a global query filter for TenantId — the background processor must process messages for all tenants. The `tenant_id` column exists for queryability and correlation.

**Infrastructure — SaveChanges integration:**

The `ApplicationDbContext` overrides `SaveChangesAsync` to:
1. Collect domain events from all tracked `AggregateRoot` entities
2. Serialize each event to an `OutboxMessage` row (using `System.Text.Json`)
3. Save aggregate state + outbox messages in the same transaction
4. Clear events from aggregates after successful commit

**Infrastructure — Background processor:**

An `OutboxProcessor` hosted as a `BackgroundService` in the ASP.NET Core process:
- Polls every 1 second for unprocessed messages
- Processes in FIFO order by `created_at`, in batches of 50
- Deserializes the event, resolves `IDomainEventHandler<TEvent>` from DI, and invokes handlers
- Marks `processed_at` on success
- Increments `retry_count` on failure, caps at 5 retries
- Uses `IServiceScopeFactory` for per-batch scoping

Future upgrade path: replace polling with PostgreSQL `LISTEN/NOTIFY` for sub-second dispatch, or switch to Debezium CDC pushing to a message broker.

### Positive Consequences

- Domain events and aggregate mutations are always consistent (same transaction)
- No message broker dependency for MVP
- Outbox table doubles as audit log for LGPD compliance
- Future migration to RabbitMQ/Kafka requires only replacing `OutboxProcessor` with a CDC connector or broker publisher
- Domain layer remains free of external dependencies

### Negative Consequences

- Polling introduces latency (up to 1 second; reducible with `LISTEN/NOTIFY`)
- Outbox table grows continuously; requires periodic cleanup of processed messages
- Handlers must be idempotent — at-least-once delivery means duplicate dispatch is possible
- `BackgroundService` runs in the API process — if the API is not running, dispatch pauses (acceptable for MVP)

## Pros and Cons of the Options

### Outbox Pattern with EF Core (same-transaction write)

- Good, because aggregate state and events are atomically consistent
- Good, because uses existing PostgreSQL database — no new infrastructure
- Good, because persisted events serve as audit trail
- Good, because future broker migration does not affect domain or application layers
- Bad, because polling adds dispatch latency
- Bad, because outbox table requires periodic maintenance

### In-memory dispatch only (fire-and-forget after SaveChanges)

- Good, because zero infrastructure overhead and zero latency
- Good, because simplest possible implementation
- Bad, because events are lost on application crash — violates at-least-once guarantee
- Bad, because no event persistence for audit or compliance
- Bad, because no retry mechanism for failed handlers

### Dedicated Event Store (EventStoreDB or Marten)

- Good, because purpose-built for event storage and replay
- Good, because supports event sourcing if the domain evolves toward it
- Bad, because introduces additional infrastructure (EventStoreDB server or Marten schema)
- Bad, because the domain is CRUD-heavy — event sourcing adds complexity without proportional benefit
- Bad, because dual-write between PostgreSQL (aggregates) and EventStoreDB (events) reintroduces the problem being solved

### Change Data Capture (Debezium)

- Good, because decouples event publication from application code
- Good, because highly scalable for high-throughput event streams
- Bad, because requires running Debezium + Kafka/Pulsar infrastructure
- Bad, because significant operational complexity for an MVP
- Bad, because CDC captures all database changes, not just domain events — requires filtering logic

## Links

- Refines [Use Clean Architecture](20260224-use-clean-architecture.md) — domain events in Domain layer, outbox in Infrastructure
- Refines [Use EF Core with PostgreSQL for Persistence](20260224-use-ef-core-with-postgresql-for-persistence.md) — outbox shares the same DbContext and transaction
- Referenced by [LGPD Data Erasure Strategy](20260303-lgpd-data-erasure-strategy.md) — erasure events persisted via outbox
