using Ledger.Core.Entities;
using Ledger.Core.Interfaces;
using Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure.Repositories;

public class HoldingRepository(LedgerDbContext db) : IHoldingRepository
{
    public async Task<IReadOnlyList<Holding>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default) =>
        await db.Holdings
            .Where(h => h.AccountId == accountId)
            .OrderBy(h => h.Symbol)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Holding>> GetAllAsync(CancellationToken ct = default) =>
        await db.Holdings.OrderBy(h => h.Symbol).ToListAsync(ct);

    public async Task<Holding> AddAsync(Holding holding, CancellationToken ct = default)
    {
        db.Holdings.Add(holding);
        await db.SaveChangesAsync(ct);
        return holding;
    }

    public async Task AddRangeAsync(IEnumerable<Holding> holdings, CancellationToken ct = default)
    {
        db.Holdings.AddRange(holdings);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Holding holding, CancellationToken ct = default)
    {
        db.Holdings.Update(holding);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        var holdings = await db.Holdings.Where(h => h.AccountId == accountId).ToListAsync(ct);
        db.Holdings.RemoveRange(holdings);
        await db.SaveChangesAsync(ct);
    }
}
