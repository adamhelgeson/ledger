using Ledger.Core.Entities;
using Ledger.Core.Interfaces;
using Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure.Repositories;

public class BalanceSnapshotRepository(LedgerDbContext db) : IBalanceSnapshotRepository
{
    public async Task<IReadOnlyList<BalanceSnapshot>> GetByAccountIdAsync(
        Guid accountId, int limit = 90, CancellationToken ct = default) =>
        await db.BalanceSnapshots
            .Where(s => s.AccountId == accountId)
            .OrderByDescending(s => s.AsOfDate)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<BalanceSnapshot?> GetLatestAsync(Guid accountId, CancellationToken ct = default) =>
        await db.BalanceSnapshots
            .Where(s => s.AccountId == accountId)
            .OrderByDescending(s => s.AsOfDate)
            .FirstOrDefaultAsync(ct);

    public async Task<BalanceSnapshot> AddAsync(BalanceSnapshot snapshot, CancellationToken ct = default)
    {
        db.BalanceSnapshots.Add(snapshot);
        await db.SaveChangesAsync(ct);
        return snapshot;
    }

    public async Task<IReadOnlyList<BalanceSnapshot>> GetAllLatestAsync(CancellationToken ct = default)
    {
        // SQLite cannot apply Max() on DateTimeOffset in SQL — group client-side.
        // Balance snapshots are a small table for personal finance, so this is acceptable.
        var all = await db.BalanceSnapshots.ToListAsync(ct);
        return all
            .GroupBy(s => s.AccountId)
            .Select(g => g.OrderByDescending(s => s.AsOfDate).First())
            .ToList()
            .AsReadOnly();
    }
}
