using Ledger.Core.Entities;
using Ledger.Core.Interfaces;
using Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure.Repositories;

public class AccountRepository(LedgerDbContext db) : IAccountRepository
{
    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct = default) =>
        await db.Accounts
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Accounts.FindAsync([id], ct);

    public async Task<Account?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await db.Accounts
            .Include(a => a.BalanceSnapshots.OrderByDescending(s => s.AsOfDate).Take(1))
            .Include(a => a.Holdings)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Account> AddAsync(Account account, CancellationToken ct = default)
    {
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);
        return account;
    }

    public async Task UpdateAsync(Account account, CancellationToken ct = default)
    {
        db.Accounts.Update(account);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await db.Accounts.FindAsync([id], ct);
        if (account is not null)
        {
            db.Accounts.Remove(account);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await db.Accounts.AnyAsync(a => a.Id == id, ct);
}
