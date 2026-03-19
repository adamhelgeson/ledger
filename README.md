# Ledger — The All-Father's Treasury

> *Forged in the fires of Nidavellir. Powered by C#, React, and an unhealthy obsession with budgeting.*

A local-first personal finance dashboard with a conversational AI agent (Heimdall). Dark mode by default. Viking / Marvel themed.

---

## Features

- **Net worth tracking** across checking, savings, credit card, brokerage, and retirement accounts
- **Transaction history** — server-side pagination, search, date range filters, category filters
- **CSV import** (The Bifrost) — auto-detects Chase and generic bank formats, deduplicates on re-import
- **Auto-categorization** — pattern-matched on import; click any category in the transaction table to override it inline
- **Spending charts** (The Ragnarök Report) — monthly breakdown by category
- **Heimdall AI chat** — powered by Claude; queries your actual transaction data to answer finance questions
- **Settings** (The Nine Realms) — add, edit, archive, or delete accounts

---

## Quick Start (Development)

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and [pnpm](https://pnpm.io/)

### Backend

```bash
cd src/Ledger.Api
dotnet watch run
# API: http://localhost:5050
# Swagger: http://localhost:5050/swagger
```

The API auto-migrates the SQLite database and seeds demo data on first startup.

### Frontend

```bash
cd client
pnpm install
pnpm dev
# App: http://localhost:3000
```

### Heimdall AI (optional)

Set your Anthropic API key before starting the backend:

```bash
export ANTHROPIC_API_KEY="sk-ant-..."
# or add to src/Ledger.Api/appsettings.json: "Heimdall": { "ApiKey": "sk-ant-..." }
```

---

## Home Media Server Deployment

Deploy Ledger as two Docker containers (API + nginx/SPA) with a persistent SQLite volume.

### Prerequisites

- Docker Engine 24+ with Compose V2 (`docker compose`)
- A host with at least 512 MB RAM

### 1. Clone and configure

```bash
git clone https://github.com/your-user/ledger.git
cd ledger
```

### 2. Set your Anthropic API key (optional — for Heimdall chat)

```bash
export ANTHROPIC_API_KEY="sk-ant-..."
```

Or create a `.env` file in the project root:

```env
ANTHROPIC_API_KEY=sk-ant-...
```

### 3. Build and start

```bash
docker compose up -d --build
```

This builds two images and starts:

| Container | Port | Description |
|---|---|---|
| `ledger-web` | `3000` | nginx serving the React SPA + proxying `/api/` to the backend |
| `ledger-api` | (internal) | ASP.NET Core API on port 5050 |

The SQLite database is persisted in the `ledger-data` Docker volume and survives container restarts/upgrades.

### 4. Open in browser

```
http://your-server-ip:3000
```

### 5. Health check

```bash
curl http://your-server-ip:3000/health
# → Healthy
```

### Upgrading

```bash
git pull
docker compose up -d --build
```

Data is preserved in the `ledger-data` volume.

### Stopping

```bash
docker compose down
```

### Backup the database

```bash
docker run --rm -v ledger-data:/data -v $(pwd):/backup alpine \
  cp /data/ledger.db /backup/ledger-backup-$(date +%Y%m%d).db
```

### Reverse proxy (optional)

To serve on port 80/443, place Ledger behind Nginx Proxy Manager, Traefik, or Caddy. Point your proxy at `ledger-web:80`. The `/api/` prefix is already handled internally by the nginx container.

---

## Architecture

```
ledger/
├── src/
│   ├── Ledger.Core/          Domain entities, repository interfaces
│   ├── Ledger.Application/   CQRS handlers (MediatR), DTOs, validators
│   ├── Ledger.Infrastructure/ EF Core, repositories, CSV parsers, Heimdall agent
│   └── Ledger.Api/           ASP.NET Core controllers, middleware
├── client/                   React + Vite + TypeScript + Tailwind
├── Dockerfile                Multi-stage build (dotnet → node → api + web targets)
├── docker-compose.yml        Production deployment
└── nginx.conf                SPA + API proxy config
```

**Tech stack:** ASP.NET Core 8, SQLite (EF Core), MediatR, FluentValidation, React 18, Vite, TanStack Query v5, Zustand v5, Tailwind CSS, Recharts, Anthropic Claude API.

---

## CSV Import

Ledger auto-detects bank format from CSV headers:

| Bank | Detection |
|---|---|
| Chase | Headers contain `Transaction Date`, `Post Date`, `Type` |
| Generic | Fallback — maps any headers with date/amount/description |

Drag and drop a CSV onto **The Bifrost** (⚡ button in the header). Duplicates are detected automatically on re-import.

---

## License

MIT
