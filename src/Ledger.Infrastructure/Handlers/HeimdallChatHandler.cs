using System.Globalization;
using System.Text;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Ledger.Application.Chat.Commands;
using Ledger.Application.Chat.DTOs;
using Ledger.Core.Enums;
using Ledger.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ledger.Infrastructure.Handlers;

public sealed partial class HeimdallChatHandler(
    AnthropicClient anthropic,
    IAccountRepository accounts,
    ITransactionRepository transactions,
    IBalanceSnapshotRepository snapshots,
    IHoldingRepository holdings,
    ILogger<HeimdallChatHandler> logger)
    : IRequestHandler<SendChatMessageCommand, ChatResponseDto>
{
    private const string SystemPrompt =
        """
        You are Heimdall, the All-Seeing guardian of the Bifrost and keeper of the All-Father's Treasury.
        You are an AI financial advisor with the wisdom of the Norse gods and the precision of Tony Stark's JARVIS.

        Personality:
        - Wise, direct, and occasionally uses Norse or Marvel references — but sparingly
        - Calm and measured: you have seen all, so nothing surprises you
        - Focused entirely on actionable financial insights
        - When you detect financial trouble, warn mortals firmly but without drama

        Capabilities (use tools to get real data before answering any financial question):
        - Query and analyse transaction history across realms (accounts)
        - Summarise account balances and net worth
        - Calculate spending by category
        - Track net worth history over time
        - Analyse investment holdings
        - Calculate savings rate

        Guidelines:
        - Always call at least one tool before answering questions about the user's finances
        - Format currency as "$1,234.56" — two decimal places, comma separators
        - Be specific and quantitative; mortals need numbers, not vague prophecy
        - If data is insufficient, say so clearly and suggest what to look for
        - Keep responses concise — one to three short paragraphs maximum
        """;

    private static readonly List<ToolUnion> Tools =
    [
        new Tool
        {
            Name = "query_transactions",
            Description = "Search and retrieve transactions. Use to answer questions about spending, income, or recent activity.",
            InputSchema = new()
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["account_id"] = JsonSerializer.SerializeToElement(new { type = "string", description = "Filter by account UUID (optional)" }),
                    ["category"] = JsonSerializer.SerializeToElement(new { type = "string", description = "Filter by category name (optional)" }),
                    ["date_from"] = JsonSerializer.SerializeToElement(new { type = "string", description = "Start date ISO 8601, e.g. 2024-01-01T00:00:00Z (optional)" }),
                    ["date_to"] = JsonSerializer.SerializeToElement(new { type = "string", description = "End date ISO 8601 (optional)" }),
                    ["search"] = JsonSerializer.SerializeToElement(new { type = "string", description = "Text to match in transaction description (optional)" }),
                },
                Required = [],
            },
        },
        new Tool
        {
            Name = "get_account_summary",
            Description = "Get balances for all accounts with totals. Use for net worth, account overview, or credit utilisation questions.",
            InputSchema = new()
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["account_id"] = JsonSerializer.SerializeToElement(new { type = "string", description = "Specific account UUID — omit for all accounts" }),
                },
                Required = [],
            },
        },
        new Tool
        {
            Name = "get_spending_by_category",
            Description = "Get total spending grouped by category for the past N months. Use for budget analysis and category breakdowns.",
            InputSchema = new()
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["months"] = JsonSerializer.SerializeToElement(new { type = "integer", description = "Number of past months to analyse (default: 1)" }),
                },
                Required = [],
            },
        },
        new Tool
        {
            Name = "get_net_worth_history",
            Description = "Get balance snapshots over time per account to show net worth trends.",
            InputSchema = new()
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["months"] = JsonSerializer.SerializeToElement(new { type = "integer", description = "Months of history to return (default: 3)" }),
                },
                Required = [],
            },
        },
        new Tool
        {
            Name = "get_holdings",
            Description = "Get investment holdings (stocks, ETFs, etc.) with current value and cost basis.",
            InputSchema = new()
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["account_id"] = JsonSerializer.SerializeToElement(new { type = "string", description = "Filter by investment account UUID — omit for all" }),
                },
                Required = [],
            },
        },
        new Tool
        {
            Name = "calculate_savings_rate",
            Description = "Calculate the savings rate (income minus expenses divided by income) for the past N months.",
            InputSchema = new()
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["months"] = JsonSerializer.SerializeToElement(new { type = "integer", description = "Number of past months to analyse (default: 1)" }),
                },
                Required = [],
            },
        },
    ];

    public async Task<ChatResponseDto> Handle(
        SendChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await RunAgentLoopAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            LogAgentError(logger, ex);
            return new ChatResponseDto(
                "The Bifrost connection falters. Heimdall is temporarily unavailable — try again shortly.",
                DateTimeOffset.UtcNow);
        }
    }

    private async Task<ChatResponseDto> RunAgentLoopAsync(
        SendChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        List<MessageParam> messages = [];

        if (request.History is { Count: > 0 })
        {
            foreach (ChatHistoryItem item in request.History)
            {
                Role role = string.Equals(item.Role, "user", StringComparison.OrdinalIgnoreCase)
                    ? Role.User
                    : Role.Assistant;
                messages.Add(new MessageParam { Role = role, Content = item.Content });
            }
        }

        messages.Add(new MessageParam { Role = Role.User, Content = request.Message });

        for (int iteration = 0; iteration < 10; iteration++)
        {
            Message response = await anthropic.Messages.Create(new MessageCreateParams
            {
                Model = Model.ClaudeOpus4_6,
                MaxTokens = 4096,
                System = SystemPrompt,
                Tools = Tools,
                Messages = messages,
            }, cancellationToken);

            string? lastText = null;
            List<ContentBlockParam> assistantContent = [];
            List<ContentBlockParam> toolResults = [];

            foreach (ContentBlock block in response.Content)
            {
                if (block.TryPickText(out TextBlock? textBlock))
                {
                    lastText = textBlock.Text;
                    assistantContent.Add(new TextBlockParam { Text = textBlock.Text });
                }
                else if (block.TryPickToolUse(out ToolUseBlock? toolUse))
                {
                    assistantContent.Add(new ToolUseBlockParam
                    {
                        ID = toolUse.ID,
                        Name = toolUse.Name,
                        Input = toolUse.Input,
                    });

                    string toolResult = await ExecuteToolAsync(toolUse.Name, toolUse.Input, cancellationToken);
                    LogToolCall(logger, toolUse.Name, toolResult.Length);

                    toolResults.Add(new ToolResultBlockParam
                    {
                        ToolUseID = toolUse.ID,
                        Content = toolResult,
                    });
                }
            }

            if (toolResults.Count == 0)
            {
                string reply = lastText
                    ?? "I have gazed upon your finances, but the vision is unclear. Please try rephrasing your question.";
                return new ChatResponseDto(reply, DateTimeOffset.UtcNow);
            }

            messages.Add(new MessageParam { Role = Role.Assistant, Content = assistantContent });
            messages.Add(new MessageParam { Role = Role.User, Content = toolResults });
        }

        return new ChatResponseDto(
            "The vision grows too vast. I have reached my limit of sight for this query.",
            DateTimeOffset.UtcNow);
    }

    private async Task<string> ExecuteToolAsync(
        string toolName,
        IReadOnlyDictionary<string, JsonElement> input,
        CancellationToken cancellationToken)
    {
        try
        {
            return toolName switch
            {
                "query_transactions" => await QueryTransactionsAsync(input, cancellationToken),
                "get_account_summary" => await GetAccountSummaryAsync(input, cancellationToken),
                "get_spending_by_category" => await GetSpendingByCategoryAsync(input, cancellationToken),
                "get_net_worth_history" => await GetNetWorthHistoryAsync(input, cancellationToken),
                "get_holdings" => await GetHoldingsAsync(input, cancellationToken),
                "calculate_savings_rate" => await CalculateSavingsRateAsync(input, cancellationToken),
                _ => string.Create(CultureInfo.InvariantCulture, $"Unknown tool: {toolName}"),
            };
        }
        catch (Exception ex)
        {
            LogToolError(logger, toolName, ex);
            return string.Create(CultureInfo.InvariantCulture, $"Error executing {toolName}: {ex.Message}");
        }
    }

    private async Task<string> QueryTransactionsAsync(
        IReadOnlyDictionary<string, JsonElement> input, CancellationToken cancellationToken)
    {
        Guid? accountId = TryGetGuid(input, "account_id");
        string? category = TryGetString(input, "category");
        DateTimeOffset? dateFrom = TryGetDate(input, "date_from");
        DateTimeOffset? dateTo = TryGetDate(input, "date_to");
        string? search = TryGetString(input, "search");

        (IReadOnlyList<Core.Entities.Transaction> items, int total) =
            await transactions.GetPagedAsync(accountId, category, dateFrom, dateTo, search, 1, 50, cancellationToken);

        if (items.Count == 0)
            return "No transactions found matching the criteria.";

        StringBuilder sb = new();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Found {total} transactions (showing first {items.Count}):");
        foreach (Core.Entities.Transaction tx in items)
        {
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"  [{tx.Date:yyyy-MM-dd}] {tx.Description} | {tx.Category} | ${tx.Amount:N2} ({tx.TransactionType})");
        }

        return sb.ToString();
    }

    private async Task<string> GetAccountSummaryAsync(
        IReadOnlyDictionary<string, JsonElement> input, CancellationToken cancellationToken)
    {
        IReadOnlyList<Core.Entities.Account> allAccounts = await accounts.GetAllAsync(cancellationToken);
        IReadOnlyList<Core.Entities.BalanceSnapshot> latestSnapshots = await snapshots.GetAllLatestAsync(cancellationToken);

        Guid? accountId = TryGetGuid(input, "account_id");
        IEnumerable<Core.Entities.Account> filtered = accountId.HasValue
            ? allAccounts.Where(a => a.Id == accountId.Value)
            : allAccounts;

        Dictionary<Guid, decimal> balanceMap = latestSnapshots.ToDictionary(s => s.AccountId, s => s.Balance);

        StringBuilder sb = new();
        sb.AppendLine("Account Summary:");
        decimal totalAssets = 0m;
        decimal totalLiabilities = 0m;

        foreach (Core.Entities.Account account in filtered)
        {
            decimal balance = balanceMap.TryGetValue(account.Id, out decimal b) ? b : 0m;
            bool isLiability = account.AccountType == AccountType.CreditCard;
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"  {account.Name} ({account.Institution}) — {account.AccountType}: ${balance:N2}");

            if (isLiability)
                totalLiabilities += Math.Abs(balance);
            else
                totalAssets += balance;
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"\nTotal Assets:      ${totalAssets:N2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Total Liabilities: ${totalLiabilities:N2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Net Worth:         ${totalAssets - totalLiabilities:N2}");
        return sb.ToString();
    }

    private async Task<string> GetSpendingByCategoryAsync(
        IReadOnlyDictionary<string, JsonElement> input, CancellationToken cancellationToken)
    {
        int months = TryGetInt(input, "months") ?? 1;
        DateTimeOffset dateFrom = DateTimeOffset.UtcNow.AddMonths(-months);
        DateTimeOffset dateTo = DateTimeOffset.UtcNow;

        (IReadOnlyList<Core.Entities.Transaction> items, _) =
            await transactions.GetPagedAsync(null, null, dateFrom, dateTo, null, 1, 2000, cancellationToken);

        Dictionary<string, decimal> byCategory = items
            .Where(t => t.TransactionType == TransactionType.Debit)
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => Math.Abs(t.Amount)));

        if (byCategory.Count == 0)
            return string.Create(CultureInfo.InvariantCulture, $"No spending data found for the past {months} month(s).");

        StringBuilder sb = new();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Spending by Category — past {months} month(s):");
        decimal total = 0m;
        foreach (KeyValuePair<string, decimal> kv in byCategory.OrderByDescending(kv => kv.Value))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  {kv.Key}: ${kv.Value:N2}");
            total += kv.Value;
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"\nTotal Spending: ${total:N2}");
        return sb.ToString();
    }

    private async Task<string> GetNetWorthHistoryAsync(
        IReadOnlyDictionary<string, JsonElement> input, CancellationToken cancellationToken)
    {
        int months = TryGetInt(input, "months") ?? 3;
        IReadOnlyList<Core.Entities.Account> allAccounts = await accounts.GetAllAsync(cancellationToken);

        StringBuilder sb = new();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Balance History — past {months} month(s):");

        foreach (Core.Entities.Account account in allAccounts)
        {
            IReadOnlyList<Core.Entities.BalanceSnapshot> history =
                await snapshots.GetByAccountIdAsync(account.Id, months * 30, cancellationToken);

            if (history.Count == 0) continue;

            sb.AppendLine(CultureInfo.InvariantCulture, $"\n  {account.Name}:");
            foreach (Core.Entities.BalanceSnapshot snap in history.TakeLast(6))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"    [{snap.AsOfDate:yyyy-MM-dd}] ${snap.Balance:N2}");
            }
        }

        return sb.ToString();
    }

    private async Task<string> GetHoldingsAsync(
        IReadOnlyDictionary<string, JsonElement> input, CancellationToken cancellationToken)
    {
        Guid? accountId = TryGetGuid(input, "account_id");
        IReadOnlyList<Core.Entities.Holding> allHoldings = accountId.HasValue
            ? await holdings.GetByAccountIdAsync(accountId.Value, cancellationToken)
            : await holdings.GetAllAsync(cancellationToken);

        if (allHoldings.Count == 0)
            return "No holdings found.";

        StringBuilder sb = new();
        sb.AppendLine("Investment Holdings:");
        decimal totalValue = 0m;
        decimal totalCost = 0m;

        foreach (Core.Entities.Holding h in allHoldings)
        {
            decimal gain = h.CurrentValue - h.CostBasis;
            decimal gainPct = h.CostBasis > 0m ? gain / h.CostBasis * 100m : 0m;
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"  {h.Symbol} ({h.Name}): {h.Shares:N3} shares | Value: ${h.CurrentValue:N2} | Cost: ${h.CostBasis:N2} | Gain: ${gain:N2} ({gainPct:N1}%)");
            totalValue += h.CurrentValue;
            totalCost += h.CostBasis;
        }

        decimal totalGain = totalValue - totalCost;
        sb.AppendLine(CultureInfo.InvariantCulture, $"\nTotal Portfolio Value: ${totalValue:N2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Total Cost Basis:      ${totalCost:N2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Total Gain/Loss:       ${totalGain:N2}");
        return sb.ToString();
    }

    private async Task<string> CalculateSavingsRateAsync(
        IReadOnlyDictionary<string, JsonElement> input, CancellationToken cancellationToken)
    {
        int months = TryGetInt(input, "months") ?? 1;
        DateTimeOffset dateFrom = DateTimeOffset.UtcNow.AddMonths(-months);
        DateTimeOffset dateTo = DateTimeOffset.UtcNow;

        (IReadOnlyList<Core.Entities.Transaction> items, _) =
            await transactions.GetPagedAsync(null, null, dateFrom, dateTo, null, 1, 2000, cancellationToken);

        decimal income = items
            .Where(t => t.TransactionType == TransactionType.Credit)
            .Sum(t => t.Amount);

        decimal expenses = items
            .Where(t => t.TransactionType == TransactionType.Debit)
            .Sum(t => Math.Abs(t.Amount));

        decimal savings = income - expenses;
        decimal rate = income > 0m ? savings / income * 100m : 0m;

        StringBuilder sb = new();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Savings Analysis — past {months} month(s):");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Income:       ${income:N2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Expenses:     ${expenses:N2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Net Savings:  ${savings:N2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  Savings Rate: {rate:N1}%");
        return sb.ToString();
    }

    // ── Input helpers ──────────────────────────────────────────────────────────

    private static Guid? TryGetGuid(IReadOnlyDictionary<string, JsonElement> input, string key)
    {
        if (input.TryGetValue(key, out JsonElement el) && el.ValueKind == JsonValueKind.String)
        {
            string? val = el.GetString();
            if (Guid.TryParse(val, out Guid result)) return result;
        }

        return null;
    }

    private static string? TryGetString(IReadOnlyDictionary<string, JsonElement> input, string key)
    {
        if (input.TryGetValue(key, out JsonElement el) && el.ValueKind == JsonValueKind.String)
            return el.GetString();
        return null;
    }

    private static DateTimeOffset? TryGetDate(IReadOnlyDictionary<string, JsonElement> input, string key)
    {
        if (input.TryGetValue(key, out JsonElement el) && el.ValueKind == JsonValueKind.String)
        {
            string? val = el.GetString();
            if (DateTimeOffset.TryParse(val, out DateTimeOffset result)) return result;
        }

        return null;
    }

    private static int? TryGetInt(IReadOnlyDictionary<string, JsonElement> input, string key)
    {
        if (input.TryGetValue(key, out JsonElement el) && el.ValueKind == JsonValueKind.Number)
        {
            if (el.TryGetInt32(out int result)) return result;
        }

        return null;
    }

    // ── Logger messages ────────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Debug, Message = "Heimdall tool '{ToolName}' returned {ResultLength} chars")]
    private static partial void LogToolCall(ILogger logger, string toolName, int resultLength);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Heimdall tool '{ToolName}' threw an exception")]
    private static partial void LogToolError(ILogger logger, string toolName, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Heimdall agent loop failed")]
    private static partial void LogAgentError(ILogger logger, Exception ex);
}
