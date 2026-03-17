namespace Ledger.Core.Entities;

public class Holding
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Shares { get; set; }
    public decimal CostBasis { get; set; }
    public decimal CurrentValue { get; set; }
    public DateTimeOffset AsOfDate { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
}
