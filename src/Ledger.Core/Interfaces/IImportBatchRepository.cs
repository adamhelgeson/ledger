using Ledger.Core.Entities;
using Ledger.Core.Enums;

namespace Ledger.Core.Interfaces;

public interface IImportBatchRepository
{
    Task<ImportBatch?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ImportBatch>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<ImportBatch> AddAsync(ImportBatch batch, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid id, ImportStatus status, CancellationToken ct = default);
}
