using Ledger.Core.Enums;

namespace Ledger.Application.Accounts.DTOs;

public record AccountDto(
    Guid Id,
    string Name,
    string Institution,
    AccountType AccountType,
    string Currency,
    bool IsActive,
    decimal? CurrentBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
