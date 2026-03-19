# Ledger — The All-Father's Treasury

> *Forged in the fires of Nidavellir. Powered by C#, React, and an unhealthy obsession with budgeting.*

A local-first personal finance dashboard with a conversational AI agent (Heimdall). Dark mode. Viking / Marvel themed. All data stays on your machine.

---

## Setup & First Run

### 1. Install prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and [pnpm](https://pnpm.io/) (`npm install -g pnpm`)

### 2. Add your Anthropic API key (for Heimdall AI chat)

Get a key at [console.anthropic.com](https://console.anthropic.com). Then run this once from the `src/Ledger.Api/` directory:

```bash
cd src/Ledger.Api
dotnet user-secrets set "Heimdall:ApiKey" "sk-ant-..."
```

This stores the key outside the repo in your OS secret store. Heimdall chat works without a key — it just returns an error message until one is set.

### 3. Start the backend

```bash
cd src/Ledger.Api
dotnet watch run
```

On first run it creates the SQLite database at `src/Ledger.Api/data/ledger.db` and seeds it with demo accounts and transactions so you have something to explore immediately.

### 4. Start the frontend

In a second terminal:

```bash
cd client
pnpm install
pnpm dev
```

Open [http://localhost:3000](http://localhost:3000).

---

## Using the App

### Dashboard

The main view shows:

- **The Vault of Asgard** — net worth, total assets, total liabilities, and monthly savings rate at the top
- **The Nine Realms** — your account cards with current balances and sparkline charts
- **The Ragnarök Report** — spending breakdown by category for the current month
- **The Sacred Timeline** — full transaction history with search, filters, and pagination

### Adding your accounts (The Nine Realms — Settings)

1. Click **Settings** in the top nav (or the gear icon on mobile)
2. Click **Add Realm**
3. Fill in the account name, institution, type (Checking / Savings / Credit Card / Brokerage / 401k), and currency
4. Click **Add Realm** to save

To edit or archive an account, hover over it in the Settings list — action buttons appear on the right.

### Importing bank statements (The Bifrost)

Ledger imports CSV files exported from your bank. Most banks have a "Download transactions" or "Export" option in their web interface.

**Supported formats:**
| Bank | How to export |
|---|---|
| Chase | Accounts → Download Account Activity → CSV |
| Any other bank | Export as CSV — Ledger auto-detects columns |

**Import steps:**

1. Download your CSV from your bank's website
2. Click the **⚡ Bifrost** button in the top-right header
3. Select the account you're importing into
4. Drag and drop your CSV file onto the upload zone (or click to browse)
5. Review the preview table — uncheck any rows you don't want to import
6. Click **Confirm Import**

Ledger automatically deduplicates — if you import the same file twice, or there's overlap between date ranges, existing transactions are skipped. It's safe to re-import.

### Transaction categories

Categories are assigned automatically on import based on the merchant name (Starbucks → Dining, Amazon → Shopping, etc.). You can override any category:

1. In **The Sacred Timeline**, click the category badge on any transaction row
2. Type the new category name
3. Press **Enter** or click away to save

Use the **Category** filter above the table to view all transactions in a specific category.

### Heimdall AI chat

Heimdall is an AI assistant that has direct access to your transaction data and can answer questions in plain English.

1. Click the **Heimdall** button in the top-right header
2. Ask anything about your finances, for example:
   - *"How much did I spend on dining last month?"*
   - *"What's my biggest spending category this year?"*
   - *"Show me all transactions over $500"*
   - *"What's my net worth?"*
   - *"How is my savings rate this month compared to last?"*

Heimdall remembers the conversation within the session — you can ask follow-up questions.

---

## Home Server Deployment (Docker)

Run Ledger permanently on a home server or NAS.

### Prerequisites

- Docker Engine 24+ with Compose V2 (`docker compose`)

### 1. Create a `.env` file with your API key

```env
ANTHROPIC_API_KEY=sk-ant-...
```

### 2. Build and start

```bash
docker compose up -d --build
```

Open [http://your-server-ip:3000](http://your-server-ip:3000).

The SQLite database lives in a Docker volume (`ledger-data`) and persists through upgrades.

### Upgrading

```bash
git pull
docker compose up -d --build
```

### Backup the database

```bash
docker run --rm -v ledger-data:/data -v $(pwd):/backup alpine \
  cp /data/ledger.db /backup/ledger-backup-$(date +%Y%m%d).db
```

### Serve on port 80 / with a domain

Place Ledger behind Nginx Proxy Manager, Traefik, or Caddy and point it at `ledger-web:80`. The `/api/` proxy is handled internally — you only need to expose port 80 from the `ledger-web` container.

---

## Resetting demo data

To wipe the database and start fresh:

```bash
bash scripts/seed.sh
```

Then restart the API — it re-migrates and re-seeds automatically.

---

## CSV format reference

If your bank's export doesn't auto-detect, Ledger's generic parser looks for columns containing these words (case-insensitive):

| Field | Accepted column names |
|---|---|
| Date | `date`, `transaction date`, `posted date` |
| Description | `description`, `merchant`, `payee`, `memo` |
| Amount | `amount`, `debit`, `credit` |

Negative amounts are treated as debits (spending); positive as credits (income/deposits).

---

## License

MIT
