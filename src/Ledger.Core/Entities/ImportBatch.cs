using Ledger.Core.Enums;

namespace Ledger.Core.Entities;

public class ImportBatch
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public ImportStatus Status { get; set; }
    public DateTimeOffset ImportedAt { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = [];
}
