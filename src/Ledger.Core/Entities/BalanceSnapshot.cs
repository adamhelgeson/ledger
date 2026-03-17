namespace Ledger.Core.Entities;

public class BalanceSnapshot
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public DateTimeOffset AsOfDate { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
}
