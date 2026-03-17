# Ledger — The All-Father's Treasury

> *"I am Heimdall. I can see all things."*

A local-first personal finance dashboard with a conversational AI agent. Dark-themed, data-dense, and unapologetically Norse.

![Screenshot placeholder](docs/screenshot-placeholder.png)

---

## The Realms

| Realm | What it is |
|---|---|
| **The Vault of Asgard** | Net worth across all accounts |
| **The Nine Realms** | Your accounts — each one a realm to govern |
| **The Sacred Timeline** | Full transaction history |
| **The Bifrost** | CSV import — bridge your bank data into the app |
| **Ragnarök Report** | Monthly spending breakdown by category |
| **Vibranium Reserves** | Your savings rate |
| **Heimdall** | AI chat agent — asks the all-seeing advisor about your finances |

---

## Architecture

```
Ledger.Core          → Domain models, interfaces (no dependencies)
Ledger.Application   → CQRS handlers, DTOs, validators (MediatR + FluentValidation)
Ledger.Infrastructure → EF Core SQLite, repositories, CSV parser, seed data
Ledger.Api           → ASP.NET Core 8 Web API (controllers, middleware, DI)
client/              → React 18 + Vite + TypeScript + Tailwind + shadcn/ui
```

See [CLAUDE.md](CLAUDE.md) for detailed architecture decisions and conventions.

---

## Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 8.0+ |
| Node.js | 20+ |
| pnpm | 9+ |
| dotnet-ef | 8.0+ (`dotnet tool install -g dotnet-ef`) |

---

## Getting Started

### 1. Clone and configure

```bash
git clone https://github.com/yourusername/ledger.git
cd ledger
cp .env.example .env
```

### 2. Run the API

```bash
cd src/Ledger.Api
dotnet watch run
```

The API will:
- Create `data/ledger.db` automatically
- Run pending migrations
- Seed sample data (5 accounts, 80+ transactions, holdings) on first run

- **API**: `http://localhost:5050`
- **Swagger**: `http://localhost:5050/swagger`
- **HTTPS**: `https://localhost:5051`

### 3. Run the frontend (Session 2+)

```bash
cd client
pnpm install
pnpm dev
# → http://localhost:3000
```

### 4. Reset the database

```bash
bash scripts/seed.sh
# Then restart the API to re-migrate and re-seed
```

---

## API Endpoints

| Method | Path | Description |
|---|---|---|
| GET | `/api/accounts` | List all accounts with latest balances |
| GET | `/api/accounts/{id}` | Get account by ID |
| POST | `/api/accounts` | Create account |
| PUT | `/api/accounts/{id}` | Update account |
| DELETE | `/api/accounts/{id}` | Delete account |
| GET | `/api/transactions` | Paginated transactions (filter by account, category, date, search) |
| POST | `/api/transactions` | Create transaction |
| GET | `/api/dashboard/stats` | Net worth, spending by category, account summaries |
| POST | `/api/import/preview` | Parse a CSV through the Bifrost (preview before importing) |
| POST | `/api/import/confirm` | Confirm and persist a parsed CSV import |
| POST | `/api/chat` | Ask Heimdall (AI agent — stub until Session 2) |

All responses follow the shape:
```json
{ "success": true, "data": { ... }, "errors": [] }
```

---

## Sample Data (The Nine Realms)

| Account | Institution | Type | Balance |
|---|---|---|---|
| Odin's Checking | Chase | Checking | ~$4,200 |
| Bifrost Savings | Ally | Savings | ~$12,000 |
| Stark Industries 401k | Fidelity | Retirement | ~$87,000 |
| Wakanda Forever Brokerage | Schwab | Brokerage | ~$23,000 |
| Mjölnir Visa | Chase | Credit Card | ~$1,847 |

---

## Development

### Project structure

```
ledger/
├── src/
│   ├── Ledger.Api/              ASP.NET Core Web API
│   ├── Ledger.Core/             Domain models, interfaces
│   ├── Ledger.Application/      CQRS handlers, DTOs, validators
│   ├── Ledger.Infrastructure/   EF Core, repositories, CSV parser
│   └── Ledger.Tests/            xUnit test project
├── client/                      React + Vite frontend
├── scripts/
│   └── seed.sh                  Reset and re-seed the database
├── .env.example                 Environment variable template
├── docker-compose.yml           Local deployment
├── CLAUDE.md                    Architecture reference (agent memory)
└── Ledger.sln
```

### Running tests

```bash
dotnet test
```

### Docker (media server deployment)

```bash
docker-compose up -d
# API:      http://localhost:5050
# Frontend: http://localhost:3000
```

---

## Roadmap

- [x] Session 1: .NET solution, EF Core, seed data, REST API
- [ ] Session 2: React frontend — dashboard, account cards, transaction table
- [ ] Session 3: Heimdall — Claude API integration for the chat agent
- [ ] Session 4: CSV import UI ("The Bifrost")
- [ ] Session 5: Charts — Ragnarök Report, balance history
- [ ] Session 6: Docker deployment config
