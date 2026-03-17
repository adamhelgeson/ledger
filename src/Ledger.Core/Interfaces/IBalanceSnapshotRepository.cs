using Ledger.Core.Entities;

namespace Ledger.Core.Interfaces;

public interface IBalanceSnapshotRepository
{
    Task<IReadOnlyList<BalanceSnapshot>> GetByAccountIdAsync(Guid accountId, int limit = 90, CancellationToken ct = default);
    Task<BalanceSnapshot?> GetLatestAsync(Guid accountId, CancellationToken ct = default);
    Task<BalanceSnapshot> AddAsync(BalanceSnapshot snapshot, CancellationToken ct = default);
    Task<IReadOnlyList<BalanceSnapshot>> GetAllLatestAsync(CancellationToken ct = default);
}
