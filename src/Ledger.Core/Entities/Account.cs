using Ledger.Core.Enums;

namespace Ledger.Core.Entities;

public class Account
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<ImportBatch> ImportBatches { get; set; } = [];
    public ICollection<BalanceSnapshot> BalanceSnapshots { get; set; } = [];
    public ICollection<Holding> Holdings { get; set; } = [];
}
