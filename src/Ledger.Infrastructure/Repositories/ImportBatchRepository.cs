using Ledger.Core.Entities;
using Ledger.Core.Enums;
using Ledger.Core.Interfaces;
using Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Infrastructure.Repositories;

public class ImportBatchRepository(LedgerDbContext db) : IImportBatchRepository
{
    public async Task<ImportBatch?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.ImportBatches.FindAsync([id], ct);

    public async Task<IReadOnlyList<ImportBatch>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default) =>
        await db.ImportBatches
            .Where(b => b.AccountId == accountId)
            .OrderByDescending(b => b.ImportedAt)
            .ToListAsync(ct);

    public async Task<ImportBatch> AddAsync(ImportBatch batch, CancellationToken ct = default)
    {
        db.ImportBatches.Add(batch);
        await db.SaveChangesAsync(ct);
        return batch;
    }

    public async Task UpdateStatusAsync(Guid id, ImportStatus status, CancellationToken ct = default)
    {
        var batch = await db.ImportBatches.FindAsync([id], ct);
        if (batch is not null)
        {
            batch.Status = status;
            await db.SaveChangesAsync(ct);
        }
    }
}
