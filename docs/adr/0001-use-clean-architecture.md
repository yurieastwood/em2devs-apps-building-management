# Use Clean Architecture

## Status

* Status: accepted

## Use Clean Architecture to organize the solution into layered projects with explicit dependency rules

* Deciders: Development team
* Date: 2026-02-24

## Context and Problem Statement

The Building Management system is a multi-tenant SaaS platform with 7 bounded contexts (Identity, Building, Resident, Announcement, Document, Visit, Incident). We need an architectural pattern that enforces separation of concerns, keeps the domain model independent from infrastructure, and supports long-term maintainability as the system grows.

## Decision Drivers

* Domain logic must remain independent of frameworks, databases, and external services
* Multi-tenant isolation requires clear boundaries between layers
* The team must be able to swap infrastructure (e.g., local file storage to Azure Blob) without touching domain code
* Testability: domain and application logic must be testable without infrastructure dependencies
* The solution should follow well-known .NET conventions for onboarding new developers

## Considered Options

* Clean Architecture (4-project structure)
* Vertical Slice Architecture
* Traditional N-Layer (Controller → Service → Repository)

## Decision Outcome

Chosen option: "Clean Architecture (4-project structure)", because it enforces the dependency rule (inner layers never reference outer layers), keeps the domain pure, and is well-supported by the .NET ecosystem with EF Core and ASP.NET Core.

### Project Structure

```
src/
├── EM2Devs.BuildingManagement.Domain/           # Entities, Value Objects, Domain Events, Interfaces
├── EM2Devs.BuildingManagement.Application/       # Use Cases, DTOs, Validation, Application Interfaces
├── EM2Devs.BuildingManagement.Infrastructure/    # EF Core, Identity, File Storage, External Services
└── EM2Devs.BuildingManagement.API/               # Controllers/Endpoints, Middleware, DI Configuration
```

### Dependency Rule

```
API → Application → Domain
API → Infrastructure → Application → Domain
```

Infrastructure references Application (to implement its interfaces) and Domain (to map entities). API references all layers but only to wire up dependency injection.

### Positive Consequences

* Domain layer has zero external dependencies — pure C# with no NuGet packages
* Swapping PostgreSQL for another database only affects Infrastructure
* Swapping local file storage for Azure Blob only affects Infrastructure
* Application layer can be fully tested with mocked interfaces
* Follows the most widely adopted .NET architecture pattern for DDD-oriented systems

### Negative Consequences

* More projects and indirection compared to a monolithic single-project approach
* Developers must understand which layer owns which responsibility
* Simple CRUD operations still require the full layer traversal (domain → application → infrastructure)

## Pros and Cons of the Options

### Clean Architecture (4-project structure)

* Good, because domain logic is completely decoupled from infrastructure
* Good, because well-established pattern with extensive .NET community support
* Good, because supports DDD tactical patterns (aggregates, value objects, domain events)
* Good, because infrastructure changes (database, storage, auth provider) don't ripple into domain
* Bad, because introduces boilerplate for simple CRUD operations
* Bad, because requires discipline to maintain layer boundaries

### Vertical Slice Architecture

* Good, because each feature is self-contained and easy to reason about
* Good, because reduces cross-feature coupling
* Bad, because lacks enforced dependency rules between layers
* Bad, because shared domain logic (multi-tenancy, audit) is harder to centralize
* Bad, because less conventional in the .NET enterprise ecosystem for DDD

### Traditional N-Layer (Controller → Service → Repository)

* Good, because simple and familiar to most .NET developers
* Good, because minimal boilerplate
* Bad, because service layer tends to become a "god class" with mixed concerns
* Bad, because domain logic leaks into services, making it hard to test in isolation
* Bad, because infrastructure changes ripple through the service layer

## Links

* Refined by [ADR-0002](0002-use-ef-core-with-postgresql-for-persistence.md)
* Refined by [ADR-0003](0003-use-asp.net-core-identity-with-jwt-bearer-for-authentication.md)
* Refined by [ADR-0004](0004-use-storage-abstraction-for-file-management.md)
