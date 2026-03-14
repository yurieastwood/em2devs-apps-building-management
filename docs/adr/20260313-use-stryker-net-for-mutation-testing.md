# Use Stryker.NET for Mutation Testing

- Status: accepted
- Deciders: Development team
- Date: 2026-03-13
- Tags: testing, quality

Technical Story: Introduce mutation testing to validate the effectiveness of the test suite and ensure edge cases are not missed by conventional code coverage metrics

## Context and Problem Statement

The project enforces an 80% code coverage threshold via Coverlet, which ensures a baseline quantity of tests. However, code coverage only measures which lines are executed, not whether the tests are actually asserting correct behavior. A test that executes a line without meaningful assertions still counts toward coverage but provides no safety net against regressions.

Mutation testing addresses this gap by introducing small code changes (mutants) and verifying that the test suite detects them. Surviving mutants reveal weak or missing assertions — areas where the code can change without any test failing.

How should we introduce mutation testing into the project to strengthen test quality beyond line/branch coverage?

## Decision Drivers

- Code coverage alone does not guarantee meaningful assertions — high coverage can coexist with weak tests
- The project follows Clean Architecture with clear layer boundaries, making it important to verify that domain invariants and business rules are truly guarded by tests
- The tooling must integrate with the existing .NET ecosystem (`dotnet tool`) without introducing unrelated runtimes
- Mutation testing runs are computationally expensive — the tool must support incremental/baseline runs for CI integration
- The tool should produce clear, actionable reports that help developers identify and fix test gaps
- Must support xUnit, which is the project's test framework

## Considered Options

- Stryker.NET
- NinjaTurtles
- VisualMutator

## Decision Outcome

Chosen option: "Stryker.NET", because it is the industry-standard mutation testing framework for .NET, actively maintained, supports xUnit and all modern .NET versions, provides rich HTML reporting, and offers incremental/baseline modes that make CI integration practical.

### Positive Consequences

- Reveals weak assertions and missing edge-case tests that code coverage metrics cannot detect
- Produces actionable HTML reports showing exactly which mutants survived and in which source files
- Incremental mode (`--since:main`) enables practical CI integration without excessive build times
- Distributed as a dotnet tool — installs via `dotnet tool restore`, consistent with the existing toolchain
- Complements the existing 80% Coverlet coverage threshold with a qualitative measure of test effectiveness

### Negative Consequences

- Full mutation runs are computationally expensive and can take significantly longer than a standard test run
- Adds a development dependency to the tool manifest
- Requires initial configuration to exclude generated code, DTOs, and other files where mutation testing adds no value
- Mutation score thresholds need to be tuned over time to avoid noisy false positives in early adoption

## Pros and Cons of the Options

### Stryker.NET

[Stryker.NET](https://github.com/stryker-mutator/stryker-net) — part of the Stryker Mutator family, the most widely adopted mutation testing framework across multiple languages

- Good, because actively maintained with regular releases and a large community
- Good, because distributed as a dotnet tool (`dotnet-stryker`), fitting the project's .NET-only toolchain
- Good, because supports xUnit, NUnit, and MSTest
- Good, because provides rich HTML reports with per-file and per-mutant detail
- Good, because supports incremental/baseline runs (`--since`, `--with-baseline`) for CI efficiency
- Good, because offers a dashboard for tracking mutation score over time
- Good, because configurable mutators — can enable/disable specific mutation categories (arithmetic, equality, logical, string, LINQ, etc.)
- Bad, because full runs are slow on large codebases (mitigated by incremental mode)
- Bad, because community-maintained, not Microsoft-official

### NinjaTurtles

[NinjaTurtles](https://github.com/ninjaturtles/NinjaTurtles) — early mutation testing tool for .NET

- Good, because one of the first mutation testing tools for .NET
- Bad, because project is abandoned — no commits or releases in years
- Bad, because does not support modern .NET (only .NET Framework)
- Bad, because no incremental mode or CI-friendly features
- Bad, because limited mutator set compared to Stryker.NET

### VisualMutator

[VisualMutator](https://github.com/visualmutator/visualmutator) — Visual Studio extension for mutation testing

- Good, because provides a Visual Studio GUI for exploring mutants
- Bad, because project is effectively unmaintained
- Bad, because only works as a Visual Studio extension — no CLI for CI integration
- Bad, because does not support modern .NET or cross-platform development
- Bad, because no support for incremental runs or automated reporting

## Known Limitations

### OpenAPI Source Generator Interceptors (Stryker Issue #3402)

Stryker.NET's internal Roslyn compilation does not support the `InterceptorsNamespaces` MSBuild property required by the `Microsoft.AspNetCore.OpenApi` source generator in .NET 10. This causes a CS9137 compile error during mutation testing.

**Workaround**: The Api `.csproj` includes a conditional MSBuild target (`DisableOpenApiSourceGenerator`) that removes the OpenAPI source generator analyzer when the `STRYKER_MUTATING` environment variable is set. All Stryker invocations (local script, pre-push hook, CI) set this variable. Runtime OpenAPI document generation is unaffected — only compile-time XML doc comment integration is disabled during mutation runs.

**Track**: https://github.com/stryker-mutator/stryker-net/issues/3402 / PR #3471. Remove the workaround target from the Api `.csproj` once the fix ships.

## Links

- Complements [Use Husky.NET for Git Hooks](20260228-use-huskynet-for-git-hooks.md) — mutation testing is integrated as a pre-push quality gate
- Stryker.NET interceptors bug: https://github.com/stryker-mutator/stryker-net/issues/3402
