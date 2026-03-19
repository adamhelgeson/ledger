using Ledger.Core.Enums;

namespace Ledger.Application.Import.DTOs;

public record ParsedTransactionDto(
    DateTimeOffset Date,
    string Description,
    decimal Amount,
    TransactionType TransactionType,
    string Category
);

public record ImportPreviewDto(
    string Filename,
    string DetectedParser,
    int RowCount,
    IReadOnlyList<ParsedTransactionDto> Rows,
    IReadOnlyList<string> Errors
);

public record ConfirmImportDto(
    Guid ImportBatchId,
    int ImportedCount,
    int SkippedCount
);
