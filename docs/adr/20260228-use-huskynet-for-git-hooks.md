# Use Husky.NET for Git Hooks

- Status: accepted
- Deciders: Development team
- Date: 2026-02-28
- Tags: dev-tools, ci

Technical Story: Enforce code quality and commit conventions automatically via git hooks, with the same validations mirrored in CI so they cannot be bypassed

## Context and Problem Statement

As the project grows and more contributors join, we need automated guardrails to enforce coding standards and commit conventions consistently. Without automated checks, developers can push poorly formatted code, non-conventional commit messages, or broken builds. Manual code review catches some of these, but it is error-prone, slow, and adds unnecessary friction.

Additionally, we want to enable full agentic development — AI coding agents (such as Claude Code) operating autonomously on feature branches, creating commits, and opening pull requests. For this workflow to be safe and trustworthy, the guardrails must be automated and enforceable at both the local (git hooks) and CI levels. Agents must not be able to bypass conventions, push broken builds, or introduce unformatted code, even when operating without human supervision.

How should we implement automated git hooks that work seamlessly for all contributors — human and AI — across macOS, Windows, and Linux?

## Decision Drivers

- The project is a pure .NET solution — contributors run `dotnet restore` and `dotnet build`, not `npm install`
- Hooks must activate automatically when contributors clone the repo and restore dependencies, with zero manual setup
- The solution must work cross-platform (macOS, Windows, Linux)
- We need to enforce Conventional Commits, Conventional Branches, code formatting, build, and test checks
- The same validations must run in CI to prevent bypass via `git commit --no-verify`
- Minimal external dependencies — avoid pulling in unrelated ecosystems (Node.js, Python) just for hook management
- Must support fully autonomous AI agent workflows — agents creating branches, commits, and PRs without human intervention must be held to the same standards automatically

## Considered Options

- Husky.NET (dotnet tool)
- Lefthook (npm/Go binary)
- Native git hooks (shell scripts + `core.hooksPath`)
- Husky (npm — original Node.js version)
- pre-commit framework (Python)

## Decision Outcome

Chosen option: "Husky.NET", because it is the only option that integrates natively with the .NET toolchain (`dotnet tool restore`), activates automatically via an MSBuild target on `dotnet restore`, works cross-platform, and requires no additional runtime (Node.js, Python) beyond what the project already uses.

### Hooks Configured

| Hook | Validation | Tool |
|---|---|---|
| `commit-msg` | Conventional Commits format | Shell script (`commit-lint.sh`) |
| `pre-push` | Conventional Branch naming | Shell script (`branch-lint.sh`) |
| `pre-commit` | Code formatting | `dotnet format --verify-no-changes` |
| `pre-commit` | Build succeeds | `dotnet build` |
| `pre-commit` | Tests pass | `dotnet test` |

### CI Mirroring

Every local hook validation is duplicated as a CI job in the GitHub Actions pipeline (`ci.yaml`), so that `--no-verify` bypasses are caught before merge.

### Positive Consequences

- Contributors get immediate feedback on commit messages, branch names, formatting, and build errors before pushing
- No manual setup required — `dotnet restore` activates hooks automatically via `Directory.Build.targets`
- Single ecosystem — stays within the .NET toolchain, no Node.js or Python runtime needed
- CI enforcement prevents bypass, ensuring all merged code meets quality standards

### Negative Consequences

- Adds a development dependency (Husky NuGet package) to the tool manifest
- Pre-commit hooks run build and tests, which adds time to each commit
- Shell scripts for commit and branch linting require bash, though Git for Windows bundles it

## Pros and Cons of the Options

### Husky.NET

[Husky.NET](https://github.com/alirezanet/Husky.Net) — .NET port of Husky, distributed as a dotnet tool

- Good, because it integrates with the .NET toolchain via `dotnet tool restore`
- Good, because auto-installs hooks via MSBuild target in `Directory.Build.targets`
- Good, because no additional runtime required beyond .NET SDK
- Good, because provides a structured task runner (`task-runner.json`) with file filtering, grouping, and platform-specific overrides
- Good, because `HUSKY=0` environment variable cleanly disables hooks in CI
- Bad, because it is a community-maintained package (not Microsoft-official)
- Bad, because adds a NuGet tool dependency

### Lefthook

[Lefthook](https://github.com/evilmartians/lefthook) — Go binary distributed via npm, Homebrew, or direct download

- Good, because language-agnostic and fast (compiled Go binary)
- Good, because supports parallel hook execution
- Good, because single YAML config file
- Bad, because primary distribution is via npm — contributors would need to run `npm install`, which .NET developers will never do
- Bad, because alternative installation methods (Homebrew, direct download) are platform-specific and not automatable via `dotnet restore`
- Bad, because introduces an unrelated ecosystem dependency for a .NET project

### Native git hooks (shell scripts + `core.hooksPath`)

Plain shell scripts committed in a `.githooks/` directory, activated via `git config core.hooksPath .githooks`

- Good, because zero external dependencies
- Good, because transparent and simple to understand
- Good, because can be automated via `Directory.Build.targets` calling `git config`
- Bad, because no built-in task runner — file filtering, staged-file detection, and parallel execution must be implemented manually
- Bad, because limited configuration — all logic lives in shell scripts, making complex workflows harder to maintain
- Bad, because no `${staged}` variable or include/exclude patterns — requires manual `git diff --cached` parsing

### Husky (Node.js original)

[Husky](https://github.com/typicode/husky) — the original Node.js git hook manager

- Good, because widely adopted with large community
- Good, because mature and well-documented
- Bad, because requires Node.js runtime and `npm install` — .NET contributors will not run this
- Bad, because introduces the entire Node.js ecosystem as a dependency for hook management
- Bad, because `package.json` and `node_modules/` add noise to a .NET project

### pre-commit framework (Python)

[pre-commit](https://pre-commit.com/) — Python-based git hook manager

- Good, because language-agnostic hook execution
- Good, because large ecosystem of community-maintained hooks
- Bad, because requires Python runtime — another dependency .NET developers may not have
- Bad, because hook definitions reference external Git repositories, adding network dependency
- Bad, because overkill for a project that only needs .NET-specific checks

## Links

- Related to [Use Clean Architecture](20260224-use-clean-architecture.md)
