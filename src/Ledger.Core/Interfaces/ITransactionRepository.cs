using Ledger.Core.Entities;

namespace Ledger.Core.Interfaces;

public interface ITransactionRepository
{
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync(
        Guid? accountId,
        string? category,
        DateTimeOffset? from,
        DateTimeOffset? toDate,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> AddRangeAsync(IEnumerable<Transaction> transactions, CancellationToken ct = default);
    Task<Transaction> AddAsync(Transaction transaction, CancellationToken ct = default);
    Task UpdateAsync(Transaction transaction, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetDistinctCategoriesAsync(CancellationToken ct = default);
    Task<decimal> GetTotalSpendingAsync(Guid? accountId, DateTimeOffset from, DateTimeOffset toDate, CancellationToken ct = default);
}
