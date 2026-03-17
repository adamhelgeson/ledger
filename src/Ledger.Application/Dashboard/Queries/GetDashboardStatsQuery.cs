using Ledger.Application.Dashboard.DTOs;
using Ledger.Core.Enums;
using Ledger.Core.Interfaces;
using MediatR;

namespace Ledger.Application.Dashboard.Queries;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public sealed class GetDashboardStatsHandler(
    IAccountRepository accounts,
    IBalanceSnapshotRepository snapshots,
    ITransactionRepository transactions)
    : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(
        GetDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        var allAccounts = await accounts.GetAllAsync(cancellationToken);
        var latestSnapshots = await snapshots.GetAllLatestAsync(cancellationToken);
        var snapshotMap = latestSnapshots.ToDictionary(s => s.AccountId, s => s.Balance);

        var creditCardTypes = new[] { AccountType.CreditCard };
        decimal totalAssets = 0m;
        decimal totalLiabilities = 0m;

        var accountSummaries = allAccounts
            .Where(a => a.IsActive)
            .Select(a =>
            {
                var balance = snapshotMap.GetValueOrDefault(a.Id, 0m);
                if (creditCardTypes.Contains(a.AccountType))
                    totalLiabilities += balance;
                else
                    totalAssets += balance;

                return new AccountSummaryDto(
                    a.Id, a.Name, a.Institution,
                    a.AccountType.ToString(), balance, a.Currency);
            })
            .ToList()
            .AsReadOnly();

        var now = DateTimeOffset.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);

        var (monthlyItems, _) = await transactions.GetPagedAsync(
            null, null, monthStart, now, null, 1, 1000, cancellationToken);

        var debits = monthlyItems.Where(t => t.TransactionType == TransactionType.Debit).ToList();
        var credits = monthlyItems.Where(t => t.TransactionType == TransactionType.Credit).ToList();

        var monthlySpending = debits.Sum(t => t.Amount);
        var monthlyIncome = credits.Sum(t => t.Amount);
        var savingsRate = monthlyIncome > 0
            ? Math.Round((monthlyIncome - monthlySpending) / monthlyIncome * 100, 1)
            : 0m;

        var totalSpending = debits.Sum(t => t.Amount);
        var spendingByCategory = debits
            .GroupBy(t => t.Category)
            .Select(g =>
            {
                var amount = g.Sum(t => t.Amount);
                var pct = totalSpending > 0 ? Math.Round(amount / totalSpending * 100, 1) : 0m;
                return new CategorySpendingDto(g.Key, amount, pct);
            })
            .OrderByDescending(c => c.Amount)
            .ToList()
            .AsReadOnly();

        return new DashboardStatsDto(
            NetWorth: totalAssets - totalLiabilities,
            TotalAssets: totalAssets,
            TotalLiabilities: totalLiabilities,
            MonthlySpending: monthlySpending,
            MonthlySavingsRate: savingsRate,
            SpendingByCategory: spendingByCategory,
            AccountSummaries: accountSummaries);
    }
}
