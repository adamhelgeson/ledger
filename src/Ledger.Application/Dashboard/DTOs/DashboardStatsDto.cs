namespace Ledger.Application.Dashboard.DTOs;

public record DashboardStatsDto(
    decimal NetWorth,
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal MonthlySpending,
    decimal MonthlySavingsRate,
    IReadOnlyList<CategorySpendingDto> SpendingByCategory,
    IReadOnlyList<AccountSummaryDto> AccountSummaries
);

public record CategorySpendingDto(string Category, decimal Amount, decimal Percentage);

public record AccountSummaryDto(
    Guid Id,
    string Name,
    string Institution,
    string AccountType,
    decimal? Balance,
    string Currency
);
