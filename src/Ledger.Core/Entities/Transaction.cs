using Ledger.Core.Enums;

namespace Ledger.Core.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public DateTimeOffset Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public string? Notes { get; set; }
    public Guid? ImportBatchId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public ImportBatch? ImportBatch { get; set; }
}
