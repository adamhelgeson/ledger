# CLAUDE.md — Ledger: The All-Father's Treasury

Agent memory and architecture reference for this project.
Keep this file updated as the codebase evolves.

---

## Project Overview

**Ledger** is a local-first personal finance dashboard with a conversational AI agent ("Heimdall").
Theme: Vikings + Marvel + tech culture. Dark mode by default.

- **API**: `http://localhost:5050` / `https://localhost:5051`
- **Frontend**: `http://localhost:3000`
- **Database**: SQLite at `src/Ledger.Api/data/ledger.db`

---

## Architecture

```
ledger/
├── src/
│   ├── Ledger.Core/          Zero-dependency domain layer
│   ├── Ledger.Application/   CQRS handlers, DTOs, validators
│   ├── Ledger.Infrastructure/ EF Core, repositories, CSV parser, seeder
│   └── Ledger.Api/           ASP.NET Core Web API, controllers, middleware
├── client/                   React + Vite frontend (Session 2+)
├── scripts/                  Dev tooling
└── src/Ledger.Tests/         xUnit tests
```

### Dependency flow
```
Core ← Application ← Infrastructure ← Api
```
- **Core** has zero NuGet dependencies
- **Application** depends only on Core + MediatR + FluentValidation
- **Infrastructure** depends on Core + Application + EF Core + CsvHelper
- **Api** depends on Application + Infrastructure; wires up DI in Program.cs

---

## Layer Responsibilities

### Ledger.Core
- Domain entities: `Account`, `Transaction`, `ImportBatch`, `BalanceSnapshot`, `Holding`
- Enums: `AccountType`, `TransactionType`, `ImportStatus`
- Repository interfaces (no implementations)
- **Rule**: No EF Core, no MediatR, no infrastructure concerns

### Ledger.Application
- MediatR queries and commands (one file per handler, co-located with the handler)
- DTOs / request records
- FluentValidation validators (co-located in the command file)
- Pipeline behaviors (`ValidationBehavior`)
- **Rule**: No EF Core, no HTTP, no CSV — pure business logic

### Ledger.Infrastructure
- `LedgerDbContext` + entity configurations (Fluent API)
- Repository implementations
- `GenericCsvParser` (static class — no DI needed)
- `DatabaseSeeder` (auto-runs on first startup in Development)
- MediatR handlers that need DB/parser access live here (`ParseCsvHandler`, `ConfirmImportHandler`)
- DI registration extension: `services.AddInfrastructure(connectionString)`

### Ledger.Api
- ASP.NET Core 8 controller-based API
- `Program.cs` wires up all services, runs migrations on startup, seeds in Development
- `ExceptionHandlingMiddleware` converts exceptions to typed `ApiResponse`
- Consistent response shape: `{ success, data, errors[] }`
- Ports: HTTP 5050, HTTPS 5051

---

## Key Patterns

### CQRS with MediatR 12.x
```csharp
// Query — co-locate record + handler in one file
public record GetAllAccountsQuery : IRequest<IReadOnlyList<AccountDto>>;

public sealed class GetAllAccountsHandler(IAccountRepository accounts, ...)
    : IRequestHandler<GetAllAccountsQuery, IReadOnlyList<AccountDto>>
{
    public async Task<IReadOnlyList<AccountDto>> Handle(...)  { ... }
}

// Controller dispatches via IMediator
var result = await mediator.Send(new GetAllAccountsQuery(), ct);
```

### API response wrapper
```csharp
// Always return ApiResponse<T> or ApiResponse
return Ok(ApiResponse<AccountDto>.Ok(result));
return NotFound(ApiResponse<AccountDto>.Fail("Account not found."));
```

### Logging — use LoggerMessage source generators (CA1848 is an error)
```csharp
public partial class MyService(ILogger<MyService> logger)
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Did {Thing}")]
    private static partial void LogDidThing(ILogger logger, string thing);
}
```

### Fluent API precision for money fields
```csharp
builder.Property(x => x.Amount)
    .HasColumnType("TEXT")
    .HasPrecision(18, 2);
```
SQLite stores decimals as TEXT — this is intentional for precision.

---

## Adding a New CSV Parser

The `GenericCsvParser` auto-detects columns by common name aliases. To support a specific bank:

1. Create `src/Ledger.Infrastructure/Csv/MyBankCsvParser.cs`
2. Implement the same signature: `public static (IReadOnlyList<ParsedTransactionDto>, IReadOnlyList<string>) Parse(Stream)`
3. Update `ParseCsvHandler` to detect which parser to use (e.g., by filename pattern or a query param)
4. Add column name aliases to `GenericCsvParser` if the bank uses a known but unlisted header

The `ParsedTransactionDto` is the normalised output — all parsers produce this shape.

---

## Theming Reference

| UI Element | Theme Name | Notes |
|---|---|---|
| App title | "Ledger — The All-Father's Treasury" | |
| Net worth | "The Vault of Asgard" | |
| Accounts list | "The Nine Realms" | Each account = a "realm" |
| Transaction list | "The Sacred Timeline" | TVA reference |
| CSV import | "The Bifrost" | Bridges bank to app |
| Spending chart | "Ragnarök Report" | |
| Credit card badge | Worthy ⚡ / Unworthy 🔨 / Banished 💀 | Based on utilization: <30% / <70% / ≥70% |
| Savings rate | "Vibranium Reserves" | |
| Budget alert | "Thanos Snapped Your Budget" | |
| Chat agent | "Heimdall" | Chat input: "Ask Heimdall about your finances..." |
| Empty state | "Nothing to see here, mortal." | |
| Loading state | "Summoning the Bifrost..." | |
| Error state | "Something went wrong in the multiverse." | |

### Design tokens (set up in Session 2)
```
Background:   #0F1117 (primary), #1A1D2E (cards)
Gold accent:  #D4A84B
Blue accent:  #4C9AFF (interactive)
Display font: Cinzel (headers, titles)
Mono font:    JetBrains Mono (numbers, amounts)
```

---

## File Conventions

- **One class per file**, filename matches class name
- **Namespace matches folder path** exactly
- **MediatR handlers** co-located with their query/command in the same file
- **FluentValidation validators** co-located in the same file as the command
- **Entity configurations** in `Infrastructure/Data/Configurations/`
- **No `var` for non-obvious types** (use explicit types for readability)

---

## Running the Project

### Backend
```bash
cd src/Ledger.Api
dotnet watch run
# API: http://localhost:5050
# Swagger: http://localhost:5050/swagger
```

### Reset and reseed
```bash
bash scripts/seed.sh
# Then start the API — it auto-migrates and seeds on startup
```

### Run tests
```bash
dotnet test
```

---

## Sessions Log

### Session 1 (2026-03-16)
- ✅ .NET solution scaffolded (Core, Application, Infrastructure, Api, Tests)
- ✅ All EF Core entities, configurations, and migrations
- ✅ Repository pattern with interfaces in Core, implementations in Infrastructure
- ✅ MediatR CQRS handlers for Accounts, Transactions, Dashboard, Import, Chat (stub)
- ✅ FluentValidation with pipeline behavior
- ✅ Generic CSV parser with auto-detect column mapping
- ✅ Seed data: 5 themed accounts, 80+ transactions, 8 holdings
- ✅ API compiles with 0 errors, 0 warnings (TreatWarningsAsErrors=true)
- ✅ InitialCreate migration generated
- ⏳ Frontend (client/) — Session 2
- ⏳ Heimdall Claude API integration — future session
