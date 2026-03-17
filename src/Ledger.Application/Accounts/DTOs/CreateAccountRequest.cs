using Ledger.Core.Enums;

namespace Ledger.Application.Accounts.DTOs;

public record CreateAccountRequest(
    string Name,
    string Institution,
    AccountType AccountType,
    string Currency = "USD"
);
