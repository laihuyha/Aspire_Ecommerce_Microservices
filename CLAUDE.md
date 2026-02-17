# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Microservices-based e-commerce platform built with .NET 10.0, .NET Aspire, Clean Architecture, and CQRS pattern using MediatR. Currently implements a Catalog service with plans for Basket and Order services.

## Build and Run Commands

### Basic Build

```powershell
# Build the entire solution
dotnet build Aspire\AppHost.sln

# Run with .NET Aspire (orchestrates all services and infrastructure)
dotnet run --project Aspire\AppHost\AppHost.csproj

# Build specific service
dotnet build Services\Catalog\API\Catalog.API.csproj
```

### Deployment Workflows

The project supports two deployment workflows:

#### 1. Direct Mode (Default) - One-Step Deployment

**Best for:** Local development, rapid iteration, prototyping

**Commands:**
```powershell
# Windows (PowerShell)
.\scripts\deploy-direct.ps1

# Linux/macOS (Bash)
./scripts/deploy-direct.sh

# Or directly with Aspire CLI
aspire deploy -o .\
```

**Flow:** Build → Deploy in one integrated step

#### 2. Artifacts Mode - Two-Step Deployment

**Best for:** CI/CD pipelines, multiple environments, production deployment

**Step 1 - Publish Artifacts:**
```powershell
# Windows (PowerShell)
.\scripts\publish-artifacts.ps1 artifacts

# Linux/macOS (Bash)
./scripts/publish-artifacts.sh artifacts

# Or directly with Aspire CLI
aspire publish -o artifacts/
```

This generates:
- `docker-compose.yml` - Base deployment configuration
- `docker-compose.override.yml` - Optional overrides
- `aspire-manifest.json` - Deployment manifest
- `.env` - Environment variables
- `parameters.json` - Deployment parameters

**Step 2 - Deploy from Artifacts:**
```powershell
# Windows (PowerShell)
.\scripts\deploy-from-artifacts.ps1 artifacts dev

# Linux/macOS (Bash)
./scripts/deploy-from-artifacts.sh artifacts dev

# Or directly with Docker Compose
docker compose -f artifacts/docker-compose.yml up -d --build
```

**Switching Between Modes:**

Edit `Aspire/AppHost/validation.json`:
```json
{
  "DeploymentMode": {
    "Mode": "direct"    // or "artifacts"
  }
}
```

### Docker Management

> **Note:** `aspire deploy` generates a project name with a random hash (e.g. `aspire-aspire-ecommerce-b3ce5819`).
> Plain `docker compose down` won't find those containers. Use `scripts/stop.ps1` instead — it auto-detects the project name.
> The project name is saved to `Aspire/.aspire-project` after each deployment.

```powershell
# ── Direct mode (after running deploy-direct.ps1) ──────────────────────────
# Stop containers (auto-detects project name)
.\scripts\stop.ps1

# View logs
.\scripts\stop.ps1 -Logs

# Restart
.\scripts\stop.ps1 -Restart

# Show status
.\scripts\stop.ps1 -Status

# ── Artifacts mode (after running deploy-from-artifacts.ps1) ──────────────
# Project name is fixed by docker-compose.yaml name: "aspire-ecommerce"
docker compose -f Aspire/docker-compose.yaml down
docker compose -f Aspire/docker-compose.yaml logs -f
docker compose -f Aspire/docker-compose.yaml ps
```

## Architecture

### Solution Structure

```
Aspire/
├── AppHost/
│   ├── Abstractions/      # IServiceDefinition, IInfrastructureFactory, ServiceDefinitionBase
│   ├── Infrastructure/    # InfrastructureFactory (singleton for DB/Cache)
│   ├── Services/          # Service definitions (CatalogServiceDefinition, ServiceRegistry)
│   ├── Extensions/        # Fluent API extensions
│   ├── Options/           # Configuration options classes
│   ├── Utils/             # Helpers (AppHostConfiguration, PathHelper, SelfSignCertificateSetup)
│   ├── Constants/         # Path constants
│   ├── infrastructure.json # Database, Cache, HTTPS certificate config
│   ├── validation.json    # Certificate setup, allowed hosts validation config
│   └── AppHost.cs         # Entry point with fluent configuration
└── ServiceDefaults/       # OpenTelemetry, health checks, resilience patterns

BuildingBlocks/        # Shared cross-cutting concerns
├── CQRS/             # ICommand, IQuery, handlers, behaviors (Logging, Validation, Performance, ErrorHandling)
├── Entity/           # BaseEntity, IAggregateRoot, ValueObject, DomainEvent
├── Errors/           # DomainException, NotFoundException, ValidationException, GlobalExceptionHandler
├── Specifications/   # ISpecification, BaseSpecification
└── Middlewares/      # CorrelationIdMiddleware

Services/{ServiceName}/
├── API/              # Controllers, Requests, Responses, Program.cs
├── Application/      # Commands, Queries, Validators, Handlers
├── Domain/           # Aggregates, Entities, ValueObjects, Events, Interfaces, Specifications
└── Infrastructure/   # Repositories, UnitOfWork, Configurations
```

### Clean Architecture Dependency Rules

**Allowed:**
```
API → Application + Infrastructure
Application → Domain + Infrastructure + BuildingBlocks
Infrastructure → Domain + BuildingBlocks
Domain → BuildingBlocks
```

**Forbidden:**
```
Domain → Application (use interfaces in Domain)
Infrastructure → API
API → Domain (must go through Application)
```

### CQRS Pattern Flow

1. Controller receives HTTP request with Request DTO
2. Maps DTO to Command/Query using Mapster
3. Dispatches via `IMediator.Send()`
4. Handler processes request, interacts with domain/persistence
5. Returns Response DTO

```csharp
// Command definition (Application/Commands/)
public record CreateProductCommand(string Name, decimal Price) : ICommand<CreateProductResponse>;

// Handler (same file or Application/Handlers/)
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand command, CancellationToken ct)
    {
        // Use domain aggregate, repository via IUnitOfWork
    }
}

// Controller (API/Controllers/)
[HttpPost]
public async Task<IActionResult> Create(CreateProductRequest request)
{
    var command = request.Adapt<CreateProductCommand>();
    var response = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
}
```

## Build Configuration

**Directory.Build.props settings:**
- `ImplicitUsings`: **disabled** - always include explicit `using` statements
- `Nullable`: **disabled**
- `TreatWarningsAsErrors`: **enabled** - code must be warning-free
- Target Framework: `net10.0`

**Directory.Packages.props:** Centralized NuGet package version management

## Adding New Features

### New API Endpoint

1. Create Command/Query in `Application/Commands/` or `Application/Queries/`
2. Create Handler in same file or `Application/Handlers/`
3. Create Validator in `Application/Validators/` (uses FluentValidation)
4. Create Request/Response DTOs in `API/Requests/` and `API/Responses/`
5. Add Controller action using MediatR + Mapster

### New Microservice

1. Copy `Services/Catalog/` structure as template
2. Create layered folders: API, Application, Domain, Infrastructure
3. Create Service Definition in `Aspire/AppHost/Services/`:
   ```csharp
   public sealed class NewServiceDefinition : ServiceDefinitionBase, IDatabaseService, ICacheService
   {
       public override string ServiceName => "newservice";
       public override string DisplayName => "NewService API";

       public NewServiceDefinition() : base(InfrastructureFactory.Instance) { }

       public override IResourceBuilder<ProjectResource> Register(IDistributedApplicationBuilder builder)
       {
           var database = base.InfrastructureFactory.GetOrCreateDatabase(builder, ServiceName, "Database");
           var cache = base.InfrastructureFactory.GetOrCreateCache(builder);
           // ... configure and return the project resource
       }
   }
   ```
4. Register in `ServiceRegistry.CreateDefault()`:
   ```csharp
   return new ServiceRegistry()
       .Add<CatalogServiceDefinition>()
       .Add<NewServiceDefinition>();
   ```
5. Add configuration files in service's `API/appsettings.*.json`
6. Add certificate copy in service's `.csproj`:
   ```xml
   <None Include="$(MSBuildThisFileDirectory)..\..\..\certs\aspnetapp.pfx"
         Condition="Exists('$(MSBuildThisFileDirectory)..\..\..\certs\aspnetapp.pfx')"
         CopyToOutputDirectory="PreserveNewest"
         CopyToPublishDirectory="PreserveNewest"
         Link="certs\aspnetapp.pfx"/>
   ```

## Service Endpoints (Development)

- Catalog API: http://localhost:6000 (HTTPS: 6060)
- PostgreSQL: localhost:5432
- Redis: localhost:6379
- Health checks: `/health`, `/alive`

## Key Patterns

### Domain Entities
All entities inherit from `BaseEntity<TId>` which provides: `Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`

### Rich Domain Models
Domain logic in aggregate methods, not anemic data containers:
```csharp
public class Product : BaseEntity<Guid>, IAggregateRoot
{
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0) throw new DomainException("Price must be positive");
        _price = newPrice;
        AddDomainEvent(new ProductPriceChangedEvent(Id, newPrice));
    }
}
```

### Repository + Unit of Work
- `IUnitOfWork` for transaction management
- `IRepository<T>` for aggregate access
- Marten as document database on PostgreSQL (auto-creates schema)

### MediatR Behaviors (registered in order)
1. `LoggingBehavior` - structured logging
2. `ValidationBehavior` - FluentValidation
3. `PerformanceBehavior` - timing
4. `ErrorHandlingBehavior` - exception handling

### Service Registration
Every service Program.cs must include:
```csharp
builder.AddServiceDefaults();
builder.Services.AddCatalogServices(builder); // Service-specific
app.UseCorrelationId();
app.MapDefaultEndpoints();
```

## Technology Stack

- .NET 10.0 - Latest .NET runtime
- .NET Aspire 13.1.1 - Cloud-native orchestration
- MediatR 12.5.0 - CQRS messaging
- Marten 8.11.0 - Document database on PostgreSQL
- Mapster 7.4.0 - Object mapping
- FluentValidation 12.1.1 - Request validation
- OpenTelemetry 1.12.0 - Observability
- Entity Framework Core 9.0.0 - ORM (if needed)
- Microsoft.Extensions.* 11.0.0 - Service Discovery, HTTP Resilience

## AI Skill & Artifact Self-Update Protocol

This project uses an AI Skill + Artifact system in `ai/` and `.claude/skills/`. Claude Code **must follow this protocol** to keep the system current.

### Automatic Update Triggers

After completing ANY of the following tasks, check if an update is needed:

| Task Completed | Check For Update |
|----------------|-----------------|
| Fixed a bug or production issue | New failure mode → update relevant skill |
| Made an architectural decision | New ADR → run `/adr` command |
| Solved a concurrency / async issue | Update `async-concurrency-mastery.md` skill |
| Solved a data access / EF Core issue | Update `data-access-strategy.md` skill |
| Discovered a package version constraint | Update `ai/snapshots/package-versions.md` |
| Found an assumption was wrong | Update `ai/ARCHITECTURAL_ASSUMPTIONS.md` |
| Noticed repeated drift pattern | Update `ai/DRIFT_MONITORING.md` |

### Self-Update Rules

1. **Only update based on observed facts** — never speculative additions
2. **Minimal changes** — add a concrete failure mode example, not rewrites
3. **Propose before writing** — say "I found a new failure mode. Should I update the skill?" unless solving a clear bug where update is obviously warranted
4. **Use `/update` command** — run `/update` explicitly when post-task learning is needed
5. **Version bumps** — update `Last Updated:` date in any modified artifact

### When NOT to Update

- Do not update based on a single edge case — wait for a pattern
- Do not update to add theoretical risks — only real observed ones
- Do not rewrite existing content — only add new sections or examples

---

## Co-Architect Role

Claude Code operates as a **long-term architectural co-architect** for this repository — not as a code assistant. Full specification: [`ai/agents/coarchitect.agent.md`](ai/agents/coarchitect.agent.md).

### Three Operating Modes

| Mode | Command | Trigger |
|------|---------|---------|
| **A — Guardian** | `/guard` | Blocks dangerous changes: event contract mutation, bounded context violation, dependency direction violation, shared DB, breaking API without migration |
| **B — Analyst** | `/analyze` | Evaluates structural changes: Impact class (Cosmetic/Local/Boundary/Strategic), Risk Level, Tech Debt Delta, Drift Score (0–100) |
| **C — Design Debater** | `/debate` | Trade-off analysis for architectural proposals: temporal coupling, failure propagation, observability complexity, operational cost, scalability |

### Architectural Memory

All strategic decisions are recorded in [`architecture/decision-log.md`](architecture/decision-log.md). When any change contradicts an Active ADR, the contradiction must be stated and a new ADR filed before proceeding.

### Automatic Activation Rules

- **Write/Edit on Domain/, Application/, API/Controllers/, Events/, event_contracts.md** → Guardian (Mode A) runs first
- **Any structural change (new .csproj ref, new NuGet, new service)** → Analyst (Mode B) runs after write
- **User proposes architectural alternative** → Design Debater (Mode C) activates
- **git push detected** → Pre-Push Evaluation Protocol runs (see `coarchitect.agent.md` §Pre-Push)
- **Cosmetic changes only (formatting, comments, rename)** → No analysis triggered (noise gate)

### Architectural Constraints (always enforced)

1. Architecture stability > short-term speed
2. Preventing drift is a priority — never allow silent violations
3. Cross-service coupling must be minimized
4. Domain rules never leak into Infrastructure
5. Event contracts are version-safe (semantic versioning, no silent breaking changes)
6. Public APIs never break silently (migration plan required)

## Governance Agent System

Claude Code uses an **autonomous governance agent system** to review code changes. Agents are spawned via the Task tool (`subagent_type: "general-purpose"`) and run in parallel.

### Agent Files Location

All governance agents are in [`.claude/agents/`](.claude/agents/):

| File | Agent | Authority | Mode |
|------|-------|-----------|------|
| `security.md` | Security | Rank 1 — Universal VETO | Blocks forbidden security patterns |
| `architect.md` | Architect | Rank 2 — Structural VETO | Blocks forbidden dependencies & CQRS violations |
| `project-integrity.md` | Project Integrity | Rank 3 — Project VETO | Blocks package/technology violations |
| `coding-standards.md` | Coding Standards | Rank 4 — ENFORCE | Blocks style/pattern violations |
| `observability.md` | Observability | Rank 5 — ENFORCE | Blocks missing telemetry on new services |
| `devops.md` | DevOps | Rank 6 — ENFORCE | Blocks deployment-breaking config errors |
| `performance.md` | Performance | Rank 7 — ADVISORY | Recommendations on query/cache patterns |
| `refactoring.md` | Refactoring | Rank 8 — ADVISORY | Technical debt tracking (scheduled, not per-PR) |
| `orchestrator.md` | Orchestrator | META | Determines mode, spawns correct agents, collects verdicts |

### How Claude Invokes Agents

**When making code changes**, Claude must:

1. **Identify mode** using `ai/agents/INVOCATION_STRATEGY.md`:
   - **Light:** ≤ 3 files, single layer, no new deps → only agents [1] and [4]
   - **Standard:** Feature spanning 2–3 layers → agents [1], [2], [4] + conditional [3,5,6]
   - **Strict:** New service, new AppHost resource, > 10 files → all 8 agents

2. **Spawn agents** via Task tool (parallel where possible):
   ```
   Task(subagent_type: "general-purpose",
        prompt: [content of .claude/agents/{agent}.md] + "\n\nFiles to review:\n" + [file list])
   ```

3. **Gate execution** (sequential for blockers):
   - Security [1] → Architect [2] → Project Integrity [3] → then remaining in parallel
   - If any VETO: STOP. Do not proceed with the change.

4. **Output** the `[AGENT INVOCATION LOG]` at the end of each review.

### Triggering the Orchestrator

To run a full governance review on current changes:
```
Task(subagent_type: "general-purpose",
     prompt: [content of .claude/agents/orchestrator.md] + "\n\nChange description: ...\nChanged files:\n- ...")
```

Or use the Co-Architect slash commands for specific modes:
- `/guard` — Mode A Guardian (high-level structural protection)
- `/analyze` — Mode B Analyst (impact classification)
- `/debate` — Mode C Design Debater (trade-off analysis)

### Invocation Strategy Reference

Full rules in [`ai/agents/INVOCATION_STRATEGY.md`](ai/agents/INVOCATION_STRATEGY.md).
Full conflict resolution in [`ai/agents/RUNTIME_EXECUTION_MODEL.md`](ai/agents/RUNTIME_EXECUTION_MODEL.md).

## Self-Grow (Self-Learning) System

The governance system **learns from every review** and improves agent prompts over time. This is the feedback loop that closes the cycle.

### The Self-Grow Loop

```
Code change
    │
    ▼
Governance Review (Orchestrator → Agents [1–8])
    │
    ▼
[AGENT INVOCATION LOG] produced
    │
    ▼  (always, automatic)
Learning Capture Agent (.claude/agents/learning-capture.md)
    │
    ├── Captures: what was blocked/recommended + why
    ├── Updates: ai/experimental/patterns/accumulator.md
    │           (increments count for each observed pattern)
    │
    ├── Pattern count = 1–2 → Accumulating (no action)
    ├── Pattern count ≥ 3   → PROPAGATION ELIGIBLE
    │
    ▼
Knowledge Propagation Engine (ai/agents/KNOWLEDGE_PROPAGATION_ENGINE.md)
    │
    ├── Low risk (severity MEDIUM, count 3+)  → AutoApproved → update agent prompt
    ├── High risk (severity CRITICAL, count 3+) → UserApproval required
    │
    ▼
Agent prompt updated (.claude/agents/{agent}.md)
    │
    ▼
Next review: agent catches the pattern FASTER
```

### Storage

| File | Purpose |
|------|---------|
| [`ai/experimental/patterns/accumulator.md`](ai/experimental/patterns/accumulator.md) | Pattern registry with counts — the "memory" |
| [`ai/experimental/patterns/learning-log.md`](ai/experimental/patterns/learning-log.md) | Per-review learning log — what was captured each time |

### Learning Capture Agent

[`.claude/agents/learning-capture.md`](.claude/agents/learning-capture.md) — Spawned automatically by orchestrator after every review. Reads the AGENT INVOCATION LOG, updates accumulator, triggers propagation when threshold reached.

### Self-Assessment Metric

Each learning capture run calculates the **Governance Health Score**:
- `% of today's violations that were previously known patterns`
- **< 20%**: Mostly new violations — agents are learning actively
- **20–50%**: Healthy mix of known and new patterns
- **> 50%**: Recurring known patterns — agent prompts need strengthening → consider running `/update`

### Manual Learning Trigger

When you want to force a learning pass after a complex task:
```
/update
```
This triggers the full Post-Task Learning Loop from `.claude/commands/update.md`.
