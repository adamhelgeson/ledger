using Ledger.Core.Entities;
using Ledger.Core.Enums;
using Ledger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ledger.Infrastructure.Seeders;

public partial class DatabaseSeeder(LedgerDbContext db, ILogger<DatabaseSeeder> logger)
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Database already seeded — skipping.")]
    private static partial void LogAlreadySeeded(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeding Ledger database with sample data...")]
    private static partial void LogSeedingStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeding complete. {Accounts} accounts, {Transactions} transactions, {Holdings} holdings.")]
    private static partial void LogSeedingComplete(ILogger logger, int accounts, int transactions, int holdings);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Accounts.AnyAsync(ct))
        {
            LogAlreadySeeded(logger);
            return;
        }

        LogSeedingStarted(logger);

        var now = DateTimeOffset.UtcNow;
        var rng = new Random(42); // deterministic seed for reproducibility

        // ── Accounts ────────────────────────────────────────────────────────────
        var checking = CreateAccount("Odin's Checking", "Chase", AccountType.Checking, now);
        var savings = CreateAccount("Bifrost Savings", "Ally", AccountType.Savings, now);
        var retirement = CreateAccount("Stark Industries 401k", "Fidelity", AccountType.Retirement401k, now);
        var brokerage = CreateAccount("Wakanda Forever Brokerage", "Schwab", AccountType.Brokerage, now);
        var creditCard = CreateAccount("Mjölnir Visa", "Chase", AccountType.CreditCard, now);

        db.Accounts.AddRange(checking, savings, retirement, brokerage, creditCard);
        await db.SaveChangesAsync(ct);

        // ── Balance Snapshots ────────────────────────────────────────────────────
        db.BalanceSnapshots.AddRange(
            new BalanceSnapshot { Id = Guid.NewGuid(), AccountId = checking.Id, Balance = 4217.83m, AsOfDate = now },
            new BalanceSnapshot { Id = Guid.NewGuid(), AccountId = savings.Id, Balance = 12045.00m, AsOfDate = now },
            new BalanceSnapshot { Id = Guid.NewGuid(), AccountId = retirement.Id, Balance = 87320.50m, AsOfDate = now },
            new BalanceSnapshot { Id = Guid.NewGuid(), AccountId = brokerage.Id, Balance = 23140.75m, AsOfDate = now },
            new BalanceSnapshot { Id = Guid.NewGuid(), AccountId = creditCard.Id, Balance = 1847.22m, AsOfDate = now }
        );
        await db.SaveChangesAsync(ct);

        // ── Holdings ─────────────────────────────────────────────────────────────
        db.Holdings.AddRange(
            // Stark Industries 401k
            new Holding { Id = Guid.NewGuid(), AccountId = retirement.Id, Symbol = "VOO", Name = "Vanguard S&P 500 ETF", Shares = 85.432m, CostBasis = 32100.00m, CurrentValue = 41200.50m, AsOfDate = now },
            new Holding { Id = Guid.NewGuid(), AccountId = retirement.Id, Symbol = "VTI", Name = "Vanguard Total Stock Market ETF", Shares = 120.000m, CostBasis = 22400.00m, CurrentValue = 27890.00m, AsOfDate = now },
            new Holding { Id = Guid.NewGuid(), AccountId = retirement.Id, Symbol = "VXUS", Name = "Vanguard Total International Stock ETF", Shares = 200.000m, CostBasis = 10800.00m, CurrentValue = 11430.00m, AsOfDate = now },
            new Holding { Id = Guid.NewGuid(), AccountId = retirement.Id, Symbol = "BND", Name = "Vanguard Total Bond Market ETF", Shares = 95.000m, CostBasis = 6500.00m, CurrentValue = 6800.00m, AsOfDate = now },

            // Wakanda Forever Brokerage
            new Holding { Id = Guid.NewGuid(), AccountId = brokerage.Id, Symbol = "AAPL", Name = "Apple Inc.", Shares = 22.500m, CostBasis = 3200.00m, CurrentValue = 4725.00m, AsOfDate = now },
            new Holding { Id = Guid.NewGuid(), AccountId = brokerage.Id, Symbol = "MSFT", Name = "Microsoft Corporation", Shares = 15.000m, CostBasis = 4500.00m, CurrentValue = 6315.00m, AsOfDate = now },
            new Holding { Id = Guid.NewGuid(), AccountId = brokerage.Id, Symbol = "NVDA", Name = "NVIDIA Corporation", Shares = 8.000m, CostBasis = 2100.00m, CurrentValue = 8240.00m, AsOfDate = now },
            new Holding { Id = Guid.NewGuid(), AccountId = brokerage.Id, Symbol = "AMZN", Name = "Amazon.com Inc.", Shares = 10.000m, CostBasis = 2800.00m, CurrentValue = 3860.75m, AsOfDate = now }
        );
        await db.SaveChangesAsync(ct);

        // ── Transactions: Odin's Checking (40 transactions) ─────────────────────
        var checkingTxs = new List<Transaction>();

        // Recurring income
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 75), "DIRECT DEPOSIT - ACME CORP", 3850.00m, "Income", TransactionType.Credit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 47), "DIRECT DEPOSIT - ACME CORP", 3850.00m, "Income", TransactionType.Credit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 20), "DIRECT DEPOSIT - ACME CORP", 3850.00m, "Income", TransactionType.Credit));

        // Groceries
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 72), "Whole Foods Market", 94.37m, "Groceries", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 65), "Trader Joe's", 67.12m, "Groceries", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 58), "Costco Wholesale", 187.44m, "Groceries", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 44), "Whole Foods Market", 78.90m, "Groceries", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 37), "Trader Joe's", 54.23m, "Groceries", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 22), "Whole Foods Market", 103.55m, "Groceries", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 8), "Kroger", 61.80m, "Groceries", TransactionType.Debit));

        // Dining
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 70), "Starbucks Coffee", 7.45m, "Dining", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 68), "Chipotle Mexican Grill", 14.87m, "Dining", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 62), "The Capital Grille", 127.50m, "Dining", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 55), "Starbucks Coffee", 6.95m, "Dining", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 49), "McDonald's", 11.23m, "Dining", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 33), "Nobu Restaurant", 210.00m, "Dining", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 14), "Starbucks Coffee", 8.10m, "Dining", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 5), "Chipotle Mexican Grill", 13.45m, "Dining", TransactionType.Debit));

        // Utilities
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 71), "Con Edison Electric", 89.12m, "Utilities", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 70), "Comcast Internet", 74.99m, "Utilities", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 41), "Con Edison Electric", 92.44m, "Utilities", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 40), "Comcast Internet", 74.99m, "Utilities", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 11), "Con Edison Electric", 81.70m, "Utilities", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 10), "Comcast Internet", 74.99m, "Utilities", TransactionType.Debit));

        // Gas
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 66), "Shell Gas Station", 52.80m, "Gas", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 43), "ExxonMobil", 48.35m, "Gas", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 18), "Chevron", 55.10m, "Gas", TransactionType.Debit));

        // Subscriptions
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 73), "Netflix", 22.99m, "Subscriptions", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 43), "Netflix", 22.99m, "Subscriptions", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 13), "Netflix", 22.99m, "Subscriptions", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 73), "Spotify Premium", 11.99m, "Subscriptions", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 43), "Spotify Premium", 11.99m, "Subscriptions", TransactionType.Debit));

        // Savings transfer
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 72), "Transfer to Bifrost Savings", 500.00m, "Transfers", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 42), "Transfer to Bifrost Savings", 500.00m, "Transfers", TransactionType.Debit));

        // Shopping
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 60), "Amazon.com", 47.88m, "Shopping", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 28), "Best Buy", 329.99m, "Shopping", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 7), "Amazon.com", 23.44m, "Shopping", TransactionType.Debit));

        // Health
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 50), "Planet Fitness", 24.99m, "Health", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 20), "Planet Fitness", 24.99m, "Health", TransactionType.Debit));
        checkingTxs.Add(MakeTx(checking.Id, DaysAgo(now, 35), "CVS Pharmacy", 18.75m, "Health", TransactionType.Debit));

        db.Transactions.AddRange(checkingTxs);

        // ── Transactions: Bifrost Savings (10 transactions) ──────────────────────
        var savingsTxs = new List<Transaction>
        {
            MakeTx(savings.Id, DaysAgo(now, 72), "Transfer from Odin's Checking", 500.00m, "Transfers", TransactionType.Credit),
            MakeTx(savings.Id, DaysAgo(now, 72), "Ally Bank Interest", 12.55m, "Income", TransactionType.Credit),
            MakeTx(savings.Id, DaysAgo(now, 60), "Emergency Fund Deposit", 200.00m, "Transfers", TransactionType.Credit),
            MakeTx(savings.Id, DaysAgo(now, 55), "Ally Bank Interest", 13.01m, "Income", TransactionType.Credit),
            MakeTx(savings.Id, DaysAgo(now, 42), "Transfer from Odin's Checking", 500.00m, "Transfers", TransactionType.Credit),
            MakeTx(savings.Id, DaysAgo(now, 42), "Ally Bank Interest", 13.44m, "Income", TransactionType.Credit),
            MakeTx(savings.Id, DaysAgo(now, 30), "Vacation Fund Deposit", 300.00m, "Transfers", TransactionType.Credit),
            MakeTx(savings.Id, DaysAgo(now, 25), "Ally Bank Interest", 14.02m, "Income", TransactionType.Credit),
            MakeTx(savings.Id, DaysAgo(now, 15), "Emergency Car Repair", 850.00m, "Transfers", TransactionType.Debit),
            MakeTx(savings.Id, DaysAgo(now, 12), "Ally Bank Interest", 10.81m, "Income", TransactionType.Credit),
        };
        db.Transactions.AddRange(savingsTxs);

        // ── Transactions: Mjölnir Visa (30 credit card transactions) ────────────
        var ccTxs = new List<Transaction>();

        // Monthly payment
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 70), "AUTOPAY - CHASE VISA", 1200.00m, "Transfers", TransactionType.Credit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 40), "AUTOPAY - CHASE VISA", 1450.00m, "Transfers", TransactionType.Credit));

        // Dining
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 74), "Nobu Restaurant", 185.00m, "Dining", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 67), "DoorDash", 34.78m, "Dining", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 61), "Starbucks Coffee", 9.25m, "Dining", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 54), "Shake Shack", 22.40m, "Dining", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 46), "UberEats", 28.99m, "Dining", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 38), "The Smith", 94.50m, "Dining", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 27), "Blank Street Coffee", 7.80m, "Dining", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 19), "Grubhub", 41.25m, "Dining", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 9), "Dig Inn", 16.70m, "Dining", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 3), "Sweetgreen", 15.95m, "Dining", TransactionType.Debit));

        // Shopping
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 72), "Amazon.com", 89.97m, "Shopping", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 63), "Apple Store", 1099.00m, "Shopping", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 52), "Target", 73.44m, "Shopping", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 45), "Amazon.com", 34.99m, "Shopping", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 31), "Zara", 128.00m, "Shopping", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 23), "Amazon.com", 56.72m, "Shopping", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 11), "Target", 44.88m, "Shopping", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 4), "Nike.com", 139.95m, "Shopping", TransactionType.Debit));

        // Subscriptions
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 74), "Amazon Prime", 14.99m, "Subscriptions", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 74), "Adobe Creative Cloud", 54.99m, "Subscriptions", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 74), "GitHub Pro", 4.00m, "Subscriptions", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 44), "Amazon Prime", 14.99m, "Subscriptions", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 44), "Adobe Creative Cloud", 54.99m, "Subscriptions", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 44), "GitHub Pro", 4.00m, "Subscriptions", TransactionType.Debit));

        // Travel/Transport
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 59), "Uber", 18.45m, "Transport", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 57), "JetBlue Airways", 387.00m, "Transport", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 29), "Lyft", 22.10m, "Transport", TransactionType.Debit));
        ccTxs.Add(MakeTx(creditCard.Id, DaysAgo(now, 6), "Uber", 14.90m, "Transport", TransactionType.Debit));

        db.Transactions.AddRange(ccTxs);
        await db.SaveChangesAsync(ct);

        LogSeedingComplete(logger, 5, checkingTxs.Count + savingsTxs.Count + ccTxs.Count, 8);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static Account CreateAccount(string name, string institution, AccountType type, DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Institution = institution,
            AccountType = type,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

    private static Transaction MakeTx(
        Guid accountId,
        DateTimeOffset date,
        string description,
        decimal amount,
        string category,
        TransactionType type) =>
        new()
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Date = date,
            Description = description,
            Amount = amount,
            Category = category,
            TransactionType = type,
            CreatedAt = DateTimeOffset.UtcNow
        };

    private static DateTimeOffset DaysAgo(DateTimeOffset from, int days) =>
        from.AddDays(-days);
}
