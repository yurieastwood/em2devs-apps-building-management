# Use Storage Abstraction for File Management

- Status: accepted
- Deciders: Development team
- Date: 2026-02-24
- Tags: infrastructure, file-storage

Technical Story: The Document Management epic (E-005) requires file upload and controlled sharing. The system must start with local filesystem storage for MVP simplicity and migrate to Azure Blob Storage for production without changing domain or application code.

## Context and Problem Statement

The Building Management system needs to store and serve documents (PDFs, images, office files) shared by building managers with residents at building or individual level. The MVP must work with local filesystem storage for simplicity, but the production target is Azure Blob Storage. How should we design the storage layer to support this transition without impacting domain logic?

## Decision Drivers

* Separation of concerns: domain logic must not depend on the storage provider
* MVP simplicity: local filesystem storage must work out of the box with no cloud account required
* Production readiness: Azure Blob Storage is the confirmed production target
* Testability: storage operations must be mockable for unit and integration tests
* Multi-tenancy: stored files must be isolated per tenant
* Security: files must not be publicly accessible — access control is enforced by the application

## Considered Options

* Interface-based storage abstraction (IFileStorageService)
* Direct Azure Blob SDK usage
* MinIO (S3-compatible, self-hosted)
* Database BLOB storage (PostgreSQL large objects)

## Decision Outcome

Chosen option: "Interface-based storage abstraction (IFileStorageService)", because it cleanly separates storage concerns from domain logic, allows local filesystem for MVP, and enables a one-line DI swap to Azure Blob Storage for production — all without any domain or application layer changes.

### Interface Design

```csharp
public interface IFileStorageService
{
    Task<StorageReference> UploadAsync(string tenantId, string containerPath, string fileName, Stream content, CancellationToken ct);
    Task<Stream> DownloadAsync(StorageReference reference, CancellationToken ct);
    Task DeleteAsync(StorageReference reference, CancellationToken ct);
    Task<bool> ExistsAsync(StorageReference reference, CancellationToken ct);
}
```

### Storage Reference (Value Object)

```csharp
public record StorageReference(string Provider, string Path);
```

The domain stores a `StorageReference` on the `Document` entity — it is opaque to the domain. The `Provider` field records which implementation stored the file (e.g., "local", "azure-blob"), enabling migration of existing files.

### Tenant Isolation

* Files are stored under a tenant-scoped path: `{tenantId}/{containerPath}/{fileName}`
* The storage service enforces this path structure — callers cannot bypass it
* Azure Blob Storage will use one container per tenant (or a shared container with tenant-prefixed paths, decided at implementation time)

### MVP Implementation

* `LocalFileStorageService`: stores files under a configurable base directory (e.g., `./storage/{tenantId}/...`)
* Registered in DI as `IFileStorageService`
* Docker volume mount in `compose.yaml` for persistence across container restarts

### Production Migration Path

1. Implement `AzureBlobStorageService` implementing `IFileStorageService`
2. Swap DI registration: `services.AddScoped<IFileStorageService, AzureBlobStorageService>()`
3. Optionally write a migration utility to move existing local files to Azure Blob
4. Zero changes in Domain, Application, or API layers

### Positive Consequences

* Domain layer has zero knowledge of storage implementation
* Local development requires no cloud account or emulator
* Swapping to Azure Blob Storage is a single DI registration change
* Storage operations are fully mockable for testing
* `StorageReference` records which provider stored each file, enabling gradual migration

### Negative Consequences

* The abstraction may not expose provider-specific features (e.g., Azure Blob SAS tokens, CDN integration) — these would require interface extension
* Local filesystem storage does not support concurrent multi-instance deployments (acceptable for MVP)
* Migration of existing files from local to Azure Blob requires a separate utility

## Pros and Cons of the Options

### Interface-based storage abstraction (IFileStorageService)

* Good, because domain and application layers are completely decoupled from storage
* Good, because local filesystem works for MVP with zero cloud dependency
* Good, because provider swap requires only a DI registration change
* Good, because fully testable with mocks
* Bad, because abstraction may hide provider-specific optimizations
* Bad, because file migration between providers requires separate tooling

### Direct Azure Blob SDK usage

* Good, because full access to Azure Blob features (SAS tokens, tiers, CDN)
* Good, because no abstraction layer overhead
* Bad, because couples application code to Azure SDK
* Bad, because local development requires Azurite emulator or cloud account
* Bad, because switching providers requires rewriting storage code throughout the application

### MinIO (S3-compatible, self-hosted)

* Good, because S3-compatible API is industry standard
* Good, because self-hosted with no cloud vendor lock-in
* Bad, because requires running an additional service in development and production
* Bad, because adds infrastructure complexity for the MVP phase
* Bad, because the confirmed production target is Azure Blob, not S3

### Database BLOB storage (PostgreSQL large objects)

* Good, because no separate storage service — everything in one database
* Good, because transactional consistency with metadata
* Bad, because PostgreSQL is not optimized for large binary storage
* Bad, because database backups grow significantly with file content
* Bad, because does not scale for production file volumes
* Bad, because streaming large files from the database is inefficient

## Links

- Refines [Use Clean Architecture](20260224-use-clean-architecture.md) — storage interface in Application layer, implementations in Infrastructure layer
- Related to [Use EF Core with PostgreSQL for Persistence](20260224-use-ef-core-with-postgresql-for-persistence.md) — Document metadata stored in PostgreSQL, file content stored via IFileStorageService
