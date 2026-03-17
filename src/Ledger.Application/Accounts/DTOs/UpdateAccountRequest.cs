namespace Ledger.Application.Accounts.DTOs;

public record UpdateAccountRequest(
    string Name,
    string Institution,
    string Currency,
    bool IsActive
);
