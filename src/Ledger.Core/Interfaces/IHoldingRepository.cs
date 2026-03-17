using Ledger.Core.Entities;

namespace Ledger.Core.Interfaces;

public interface IHoldingRepository
{
    Task<IReadOnlyList<Holding>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<Holding>> GetAllAsync(CancellationToken ct = default);
    Task<Holding> AddAsync(Holding holding, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Holding> holdings, CancellationToken ct = default);
    Task UpdateAsync(Holding holding, CancellationToken ct = default);
    Task DeleteByAccountIdAsync(Guid accountId, CancellationToken ct = default);
}
