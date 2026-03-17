using Ledger.Core.Entities;
using Ledger.Core.Interfaces;
using Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure.Repositories;

public class TransactionRepository(LedgerDbContext db) : ITransactionRepository
{
    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync(
        Guid? accountId,
        string? category,
        DateTimeOffset? from,
        DateTimeOffset? toDate,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Transactions.Include(t => t.Account).AsQueryable();

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(t => t.Category == category);
        if (from.HasValue)
            query = query.Where(t => t.Date >= from.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.Date <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Description.Contains(search) || t.Category.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items.AsReadOnly(), total);
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Transaction>> AddRangeAsync(
        IEnumerable<Transaction> transactions,
        CancellationToken ct = default)
    {
        var list = transactions.ToList();
        db.Transactions.AddRange(list);
        await db.SaveChangesAsync(ct);
        return list.AsReadOnly();
    }

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken ct = default)
    {
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(ct);
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken ct = default)
    {
        db.Transactions.Update(transaction);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var t = await db.Transactions.FindAsync([id], ct);
        if (t is not null)
        {
            db.Transactions.Remove(t);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<string>> GetDistinctCategoriesAsync(CancellationToken ct = default) =>
        await db.Transactions
            .Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalSpendingAsync(
        Guid? accountId, DateTimeOffset from, DateTimeOffset toDate,
        CancellationToken ct = default)
    {
        var query = db.Transactions.Where(t => t.Date >= from && t.Date <= toDate);
        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);
        return await query.SumAsync(t => t.Amount, ct);
    }
}
