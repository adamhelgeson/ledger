namespace Ledger.Application.Transactions.DTOs;

public record TransactionFilterDto(
    Guid? AccountId = null,
    string? Category = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 25
);
