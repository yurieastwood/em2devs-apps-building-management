# Use ASP.NET Core Identity with JWT Bearer for Authentication

## Status

* Status: accepted

## Use ASP.NET Core Identity for user management and JWT Bearer tokens for API authentication in a manager-only MVP

* Deciders: Development team
* Date: 2026-02-24

Technical Story: The multi-tenant Building Management system needs authentication that supports role-based access control, multi-tenant isolation via token claims, and a stateless API design suitable for horizontal scaling.

## Context and Problem Statement

The system is a multi-tenant REST API where building managers authenticate and perform operations scoped to their tenant. The MVP is manager-only (residents do not have API access yet). We need an authentication mechanism that embeds tenant context, supports role-based authorization, and works with stateless API deployments. How should we implement authentication and authorization?

## Decision Drivers

* Multi-tenant isolation: TenantId must be available in every request context without database lookups per request
* Stateless API: the system must scale horizontally without session affinity
* Role-based access: Manager role in MVP, with Owner and Renter roles planned for later phases
* .NET ecosystem: prefer built-in ASP.NET Core capabilities to reduce third-party dependencies
* Security: token expiry, refresh rotation, and signing key management must follow industry standards
* Manager-only MVP: initial scope is limited — avoid over-engineering for future resident auth

## Considered Options

* ASP.NET Core Identity + JWT Bearer tokens
* IdentityServer / Duende IdentityServer (OpenID Connect)
* Auth0 / Azure AD B2C (external identity provider)
* Cookie-based authentication

## Decision Outcome

Chosen option: "ASP.NET Core Identity + JWT Bearer tokens", because it provides built-in user management (registration, password hashing, role assignment) with stateless JWT tokens that embed TenantId and role claims, all without external service dependencies or licensing costs.

### Token Strategy

* **Access tokens**: 15-minute expiry, signed with RS256 (asymmetric keys)
* **Refresh tokens**: 7-day expiry, stored server-side, single-use with rotation
* **Claims**: `sub` (user ID), `tenant_id`, `role`, `email`
* Refresh token reuse detection: if a refresh token is used twice, all tokens for that user are revoked

### Role Model (MVP)

* `Manager`: full CRUD on all resources within their tenant
* Future phases will add `Owner` and `Renter` roles with restricted permissions

### Multi-Tenant Integration

* `TenantId` is embedded as a JWT claim at login time
* The authentication middleware extracts `TenantId` from the token and sets it in a scoped `ITenantContext` service
* EF Core global query filters (see ADR-0002) consume `ITenantContext` to scope all queries
* No cross-tenant token is possible — a user belongs to exactly one tenant

### Positive Consequences

* Zero external service dependency — Identity is part of ASP.NET Core
* TenantId travels with every request via JWT claims — no per-request database lookup for tenant resolution
* RS256 signing allows token verification without sharing the private key (useful for future microservice extraction)
* Refresh token rotation with reuse detection follows OWASP best practices
* Role-based authorization uses standard `[Authorize(Roles = "Manager")]` attributes

### Negative Consequences

* JWT tokens cannot be revoked before expiry (mitigated by short 15-minute TTL and refresh token revocation)
* ASP.NET Core Identity stores users in the same database — shared schema means Identity tables share the tenant database
* RS256 key management requires secure key storage (mitigated by using .NET Data Protection or Azure Key Vault in production)
* Adding OAuth2/OIDC flows later (e.g., for resident mobile apps) will require extending this setup

## Pros and Cons of the Options

### ASP.NET Core Identity + JWT Bearer tokens

* Good, because built into ASP.NET Core with no external dependencies
* Good, because embeds TenantId and role claims in a stateless token
* Good, because well-documented and widely adopted in the .NET ecosystem
* Good, because no licensing costs
* Bad, because JWT tokens cannot be individually revoked before expiry
* Bad, because Identity tables are coupled to the application database

### IdentityServer / Duende IdentityServer (OpenID Connect)

* Good, because full OAuth2/OIDC compliance out of the box
* Good, because supports multiple client types (SPA, mobile, API-to-API)
* Bad, because Duende requires a commercial license for production use
* Bad, because significant complexity for a manager-only MVP
* Bad, because additional deployment artifact to manage

### Auth0 / Azure AD B2C (external identity provider)

* Good, because offloads all identity management to a managed service
* Good, because built-in MFA, social login, and compliance features
* Bad, because introduces an external service dependency and associated costs
* Bad, because TenantId claim customization requires per-provider configuration
* Bad, because vendor lock-in risk
* Bad, because contradicts AGPL philosophy of self-contained open-source deployment

### Cookie-based authentication

* Good, because simple to implement for server-rendered apps
* Bad, because not suitable for stateless API consumption
* Bad, because requires session affinity or distributed cache for horizontal scaling
* Bad, because not suitable for future mobile client consumption

## Links

* Refines [ADR-0001](0001-use-clean-architecture.md) — auth middleware lives in API layer, Identity implementation in Infrastructure layer
* Related to [ADR-0002](0002-use-ef-core-with-postgresql-for-persistence.md) — Identity tables share the PostgreSQL database
