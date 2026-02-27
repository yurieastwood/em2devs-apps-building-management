# Use EF Core with PostgreSQL for Persistence

- Status: accepted
- Deciders: Development team
- Date: 2026-02-24
- Tags: persistence, infrastructure

Technical Story: The system requires a relational database to support the highly relational domain model (Buildings > Units > Residents, cross-entity audience targeting, multi-tenant data isolation, LGPD compliance).

## Context and Problem Statement

The Building Management system has a highly relational domain: buildings contain units, units have residents, announcements target multiple entities, documents have access control lists, and visits link visitors to units and residents. Multi-tenant data isolation is a hard constraint, and LGPD compliance requires precise data deletion (right to erasure). Which persistence technology and strategy best supports these requirements?

## Decision Drivers

* Relational integrity: the domain has many cross-entity relationships and referential constraints
* Multi-tenant isolation: a tenant must never see or affect another tenant's data
* LGPD/GDPR compliance: right to erasure requires cascading data operations with referential awareness
* Query flexibility: announcement audience targeting requires complex filtering (by building, unit, individual)
* .NET ecosystem maturity: the ORM must be well-supported with .NET 10
* Soft-delete support: all aggregates use soft-delete for audit and history preservation
* Audit columns: CreatedAt, UpdatedAt, CreatedBy on all entities

## Considered Options

* EF Core + PostgreSQL (shared schema, TenantId discriminator)
* EF Core + SQL Server
* Dapper + PostgreSQL (micro-ORM)
* MongoDB (document store)

## Decision Outcome

Chosen option: "EF Core + PostgreSQL (shared schema, TenantId discriminator)", because it provides the best balance of relational integrity, multi-tenant isolation via global query filters, LGPD-friendly cascading operations, mature .NET tooling, and cost-effectiveness (open-source database).

### Multi-Tenancy Strategy

* **Shared schema** with a `TenantId` column on every aggregate root table
* EF Core **global query filters** automatically apply `WHERE TenantId = @currentTenant` to all queries
* `TenantId` is extracted from the authenticated user's JWT claims — never from request body or query parameters
* A `SaveChanges` interceptor ensures `TenantId` is always set on new entities and never modified on existing ones

### Soft-Delete Strategy

* All entities have an `IsDeleted` boolean and `DeletedAt` timestamp
* A `SaveChanges` interceptor converts `Delete` operations to `Update` with `IsDeleted = true`
* Global query filters exclude soft-deleted records by default
* Explicit `.IgnoreQueryFilters()` available for admin/audit queries

### Audit Columns

* `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` on all entities
* Populated automatically via a `SaveChanges` interceptor

### ID Strategy

* ULID-ordered GUIDs (`Ulid.NewUlid().ToGuid()`) for primary keys
* Provides uniqueness without coordination (no sequence contention in multi-tenant scenarios)
* B-tree friendly ordering for PostgreSQL index performance

### Positive Consequences

* Referential integrity enforced at the database level (foreign keys, cascades)
* Global query filters guarantee tenant isolation without developer discipline per query
* EF Core migrations provide version-controlled schema evolution
* PostgreSQL row-level security can be added as an additional defense layer if needed
* Open-source database with no licensing cost

### Negative Consequences

* Shared schema means a noisy neighbor risk at extreme scale (mitigated by connection pooling and query optimization)
* EF Core change tracking adds overhead for bulk operations (mitigated by using raw SQL or Dapper for specific hot paths)
* Schema migrations must be carefully managed to avoid downtime in multi-tenant deployments

## Pros and Cons of the Options

### EF Core + PostgreSQL (shared schema)

* Good, because global query filters enforce tenant isolation automatically
* Good, because referential integrity is enforced at the database level
* Good, because LGPD cascading erasure is natively supported via foreign key cascades
* Good, because PostgreSQL is open-source and cost-effective
* Good, because EF Core is the most mature .NET ORM with excellent migration tooling
* Bad, because shared schema has noisy neighbor risk at extreme scale
* Bad, because EF Core change tracking has overhead for bulk operations

### EF Core + SQL Server

* Good, because same EF Core benefits as PostgreSQL option
* Good, because excellent Azure integration if deploying to Azure
* Bad, because licensing costs for production use
* Bad, because no advantage over PostgreSQL for this domain's requirements

### Dapper + PostgreSQL (micro-ORM)

* Good, because maximum query performance with no ORM overhead
* Good, because full SQL control for complex queries
* Bad, because no change tracking — multi-tenancy, soft-delete, and audit must be manually implemented per query
* Bad, because no migration tooling — schema evolution must be managed separately
* Bad, because significantly more boilerplate code for CRUD operations

### MongoDB (document store)

* Good, because flexible schema for evolving document structures
* Good, because horizontal scaling is straightforward
* Bad, because the domain is highly relational — denormalization leads to data consistency issues
* Bad, because multi-document transactions are expensive and limited
* Bad, because LGPD cascading erasure across denormalized documents is error-prone
* Bad, because announcement audience targeting requires joins that document stores handle poorly
* Bad, because EF Core MongoDB provider is community-maintained with limited feature support

## Links

- Refines [Use Clean Architecture](20260224-use-clean-architecture.md) — persistence lives in the Infrastructure layer
