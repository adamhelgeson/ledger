#!/usr/bin/env bash
# ── scripts/seed.sh ────────────────────────────────────────────────────────────
# Drops the existing SQLite database so the next API startup will re-migrate
# and re-seed with the full sample dataset.
#
# Usage: bash scripts/seed.sh
# Then:  cd src/Ledger.Api && dotnet watch run
#        The seeder runs automatically on first startup when no accounts exist.
# ──────────────────────────────────────────────────────────────────────────────

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DATA_DIR="$REPO_ROOT/src/Ledger.Api/data"
DB_FILE="$DATA_DIR/ledger.db"

echo "⚡ Ledger — Resetting the All-Father's Treasury..."

if [ -f "$DB_FILE" ]; then
  echo "  → Removing database: $DB_FILE"
  rm -f "$DB_FILE" "$DB_FILE-shm" "$DB_FILE-wal"
  echo "  → Removed."
else
  echo "  → No existing database found."
fi

mkdir -p "$DATA_DIR"

echo ""
echo "✅ Done. The treasury has been cleared."
echo "   Start the API to re-migrate and seed:"
echo ""
echo "     cd src/Ledger.Api && dotnet watch run"
echo ""
