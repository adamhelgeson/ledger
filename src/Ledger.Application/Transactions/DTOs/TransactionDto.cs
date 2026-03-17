using Ledger.Core.Enums;

namespace Ledger.Application.Transactions.DTOs;

public record TransactionDto(
    Guid Id,
    Guid AccountId,
    string AccountName,
    DateTimeOffset Date,
    string Description,
    decimal Amount,
    string Category,
    TransactionType TransactionType,
    string? Notes,
    Guid? ImportBatchId,
    DateTimeOffset CreatedAt
);
