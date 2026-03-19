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

All parsers live in `src/Ledger.Infrastructure/Csv/` and implement `ICsvParser`:

```csharp
public interface ICsvParser
{
    string Name { get; }                           // e.g. "Chase", "Generic"
    bool CanParse(IReadOnlyList<string> headers);  // lowercase-trimmed header list
    (IReadOnlyList<ParsedTransactionDto> Rows, IReadOnlyList<string> Errors) Parse(Stream csvStream);
}
```

`CsvParserFactory.Select(stream)` peeks the header row and returns the first matching parser.
Parsers are tested in registration order inside `CsvParserFactory.Parsers[]` — add new parsers **before** `GenericCsvParser` (which always returns `true` from `CanParse`).

### Steps to add a new bank parser

1. Create `src/Ledger.Infrastructure/Csv/MyBankCsvParser.cs`
2. Implement `ICsvParser` — set `Name`, define `CanParse` using distinctive headers, implement `Parse`
3. Register it in `CsvParserFactory.Parsers[]` before `GenericCsvParser`
4. The parser is auto-detected at runtime — no controller or handler changes needed

### Currently registered parsers

| Parser | `Name` | Detection criteria |
|---|---|---|
| `ChaseCsvParser` | `"Chase"` | Has "transaction date" **and** "post date" **and** "type" |
| `GenericCsvParser` | `"Generic"` | Fallback — always matches |

### Deduplication

`ConfirmImportHandler` filters incoming rows against existing transactions (same account, same date range) using a `(date\|amount\|description)` hash. Duplicate rows are skipped and counted in `ConfirmImportDto.SkippedCount`.

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
- ✅ Frontend (client/) — Session 2
- ⏳ Heimdall Claude API integration — future session

### Session 2 (2026-03-18)
- ✅ React + Vite + TypeScript + Tailwind CSS design system
- ✅ VaultOfAsgard (net worth hero), NineRealms (account cards + sparklines), RagnarökReport (Recharts spending chart)
- ✅ SacredTimeline (transactions table with sorting, server pagination, filters, search)
- ✅ TransactionFilters (debounced search, quick date ranges, realm/category pills)
- ✅ AppHeader (sticky header with Heimdall + Bifrost + refresh actions)
- ✅ HeimdallChat (slide-in AI chat panel)
- ✅ TanStack Query v5, Zustand v5, fontsource fonts (Cinzel, JetBrains Mono)
- ✅ Worthiness badges (⚡ Worthy / 🔨 Unworthy / 💀 Banished) for credit cards

### Session 3 (2026-03-18)
- ✅ `ICsvParser` interface — all parsers implement `CanParse` + `Parse` + `Name`
- ✅ `ChaseCsvParser` — auto-detected from "Transaction Date / Post Date / Type" headers; maps Chase categories
- ✅ `CsvParserFactory` — peeks header row, selects best parser; Generic is always fallback
- ✅ Deduplication in `ConfirmImportHandler` — skips rows matching (date|amount|description) of existing transactions
- ✅ `ImportPreviewDto.DetectedParser` — parser name surfaced to UI
- ✅ `ConfirmImportDto.SkippedCount` — duplicate count returned to UI
- ✅ Bifrost frontend: drag-and-drop upload zone with rainbow shimmer hover animation
- ✅ Import preview table with per-row checkboxes, parse warnings, select-all
- ✅ Full import flow modal (upload → preview → confirm → success/error states)
- ✅ "X transactions arrived safely in [realm name]" success message
- ✅ Bifrost button in AppHeader; `bifrostOpen` state in Zustand store

### Session 4 (2026-03-18)
- ✅ `Anthropic` NuGet package (v12.9.0) added to `Ledger.Infrastructure`
- ✅ `HeimdallChatHandler` in Infrastructure — full agentic loop replacing Application placeholder
- ✅ Six Claude tools: `query_transactions`, `get_account_summary`, `get_spending_by_category`, `get_net_worth_history`, `get_holdings`, `calculate_savings_rate`
- ✅ Manual tool-use loop (call → execute tool → append result → repeat) with 10-iteration safety cap
- ✅ `AnthropicClient` registered as singleton; reads from `Heimdall:ApiKey` config or `ANTHROPIC_API_KEY` env var
- ✅ Conversation history forwarded from frontend to backend on every turn
- ✅ `sendChatMessage(text, history)` updated in api.ts; `HeimdallChat.tsx` passes prior messages as history
- ✅ Model: `claude-opus-4-6`; max tokens per turn: 4096

### Heimdall API key setup
```bash
# User secrets (dev) — run from src/Ledger.Api/
dotnet user-secrets set "Heimdall:ApiKey" "sk-ant-api03-..."

# Or set environment variable
export ANTHROPIC_API_KEY="sk-ant-api03-..."
```

### Session 5 (2026-03-18)
- ✅ `UpdateTransactionCategoryCommand` — CQRS command + handler for inline category override
- ✅ `PATCH /api/transactions/{id}/category` endpoint on `TransactionsController`
- ✅ `TransactionCategorizer` — pattern-based auto-categorization (21 categories, 100+ keyword patterns, regex compiled)
- ✅ `ConfirmImportHandler` injects categorizer: uses parser-assigned category if set, falls back to pattern match
- ✅ `GET /health` health check endpoint (`MapHealthChecks`)
- ✅ `currentView: 'dashboard' | 'settings'` added to Zustand store; `App.tsx` routes between views
- ✅ Settings nav pill in `AppHeader`; mobile Settings icon button
- ✅ `SettingsPage` with RealmManager + About section ("Forged in Nidavellir...")
- ✅ `RealmManager` — account list with add/edit/archive/delete, archived accounts section
- ✅ `AccountFormModal` — create/edit account form (name, institution, type, currency)
- ✅ Inline category editing in `SacredTimeline` — click badge → input → Enter/blur to save
- ✅ `transactionsApi.updateCategory` added to `api.ts`
- ✅ `Input` component updated to use `forwardRef` (fixes ref prop TypeScript error)
- ✅ `@types/node` + `vite-env.d.ts` added to fix frontend TypeScript config errors
- ✅ `Dockerfile` — multi-stage build (dotnet-build, node-build, api runtime, web/nginx targets)
- ✅ `docker-compose.yml` — api + web services, `ledger-data` volume, health checks, `ANTHROPIC_API_KEY` passthrough
- ✅ `nginx.conf` — SPA fallback, `/api/` and `/health` proxy to api container, asset caching
- ✅ `README.md` — full deployment guide for home media server, Docker instructions, backup recipe
