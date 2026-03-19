using CsvHelper;
using CsvHelper.Configuration;
using Ledger.Application.Import.DTOs;
using Ledger.Core.Enums;
using System.Globalization;

namespace Ledger.Infrastructure.Csv;

/// <summary>
/// Parser for Chase Bank CSV exports.
/// Expected headers: Transaction Date, Post Date, Description, Category, Type, Amount, Memo
/// Amount is negative for debits (purchases/fees), positive for credits (payments/refunds).
/// </summary>
public class ChaseCsvParser : ICsvParser
{
    public string Name => "Chase";

    public bool CanParse(IReadOnlyList<string> headers) =>
        headers.Contains("transaction date") &&
        headers.Contains("post date") &&
        headers.Contains("type");

    public (IReadOnlyList<ParsedTransactionDto> Rows, IReadOnlyList<string> Errors) Parse(Stream csvStream)
    {
        var rows = new List<ParsedTransactionDto>();
        var errors = new List<string>();

        using var reader = new StreamReader(csvStream, leaveOpen: true);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = ctx => errors.Add($"Row {ctx.Context.Parser?.Row}: bad data — {ctx.RawRecord}"),
            TrimOptions = TrimOptions.Trim,
        };

        using var csv = new CsvReader(reader, config);

        try
        {
            csv.Read();
            csv.ReadHeader();
        }
        catch (Exception ex)
        {
            errors.Add($"Failed to read CSV header: {ex.Message}");
            return (rows.AsReadOnly(), errors.AsReadOnly());
        }

        int rowNum = 1;
        while (csv.Read())
        {
            rowNum++;
            try
            {
                var dateStr = csv.GetField("Transaction Date") ?? "";
                var description = csv.GetField("Description") ?? "";
                var chaseCategory = csv.GetField("Category") ?? "";
                var type = csv.GetField("Type") ?? "";
                var amountStr = csv.GetField("Amount") ?? "0";

                if (!TryParseDate(dateStr, out DateTimeOffset date))
                {
                    errors.Add($"Row {rowNum}: could not parse date '{dateStr}'");
                    continue;
                }

                description = SanitizeString(description);
                if (string.IsNullOrWhiteSpace(description))
                {
                    errors.Add($"Row {rowNum}: empty description — skipped");
                    continue;
                }

                if (!TryParseDecimal(amountStr, out decimal raw))
                {
                    errors.Add($"Row {rowNum}: could not parse amount '{amountStr}'");
                    continue;
                }

                decimal amount = Math.Abs(raw);
                TransactionType txType = DetermineType(type, raw);
                string category = MapChaseCategory(chaseCategory, description);

                rows.Add(new ParsedTransactionDto(date, description, amount, txType, category));
            }
            catch (Exception ex)
            {
                errors.Add($"Row {rowNum}: unexpected error — {ex.Message}");
            }
        }

        return (rows.AsReadOnly(), errors.AsReadOnly());
    }

    private static TransactionType DetermineType(string chaseType, decimal rawAmount)
    {
        // Chase "Type" column takes priority; fall back to sign of Amount
        return chaseType.ToLowerInvariant() switch
        {
            "sale" => TransactionType.Debit,
            "fee" => TransactionType.Debit,
            "return" => TransactionType.Credit,
            "payment" => TransactionType.Credit,
            "adjustment" => rawAmount >= 0 ? TransactionType.Credit : TransactionType.Debit,
            _ => rawAmount >= 0 ? TransactionType.Credit : TransactionType.Debit,
        };
    }

    private static string MapChaseCategory(string chaseCategory, string description)
    {
        return chaseCategory.Trim() switch
        {
            "Food & Drink" => "Dining",
            "Restaurants" => "Dining",
            "Groceries" => "Groceries",
            "Gas" => "Gas",
            "Shopping" => "Shopping",
            "Health & Wellness" => "Health",
            "Entertainment" => "Subscriptions",
            "Travel" => "Transport",
            "Automotive" => "Transport",
            "Bills & Utilities" => "Utilities",
            "Home" => "Housing",
            "Mortgage & Rent" => "Housing",
            "Payment" => "Transfers",
            "Transfer" => "Transfers",
            "Fees & Adjustments" => "Other",
            "Personal" => "Other",
            _ => CategorizeDescription(description),
        };
    }

    private static string CategorizeDescription(string desc)
    {
        var lower = desc.ToLowerInvariant();
        if (ContainsAny(lower, "grocery", "whole foods", "trader joe", "kroger", "safeway", "aldi", "costco"))
            return "Groceries";
        if (ContainsAny(lower, "restaurant", "cafe", "coffee", "starbucks", "mcdonald", "chipotle", "doordash", "ubereats", "pizza", "sushi", "burger"))
            return "Dining";
        if (ContainsAny(lower, "netflix", "spotify", "hulu", "disney", "apple music", "amazon prime", "github", "adobe", "subscription"))
            return "Subscriptions";
        if (ContainsAny(lower, "shell", "exxon", "chevron", "bp ", "mobil", "gas station", "fuel"))
            return "Gas";
        if (ContainsAny(lower, "electric", "utility", "water bill", "internet", "comcast", "xfinity", "att ", "verizon", "t-mobile"))
            return "Utilities";
        if (ContainsAny(lower, "amazon", "target", "best buy", "apple store", "walmart", "home depot"))
            return "Shopping";
        if (ContainsAny(lower, "cvs", "walgreens", "pharmacy", "doctor", "copay", "medical", "dental", "gym", "fitness"))
            return "Health";
        if (ContainsAny(lower, "uber", "lyft", "transit", "parking", "toll", "auto", "car wash"))
            return "Transport";
        if (ContainsAny(lower, "direct deposit", "payroll", "salary", "paycheck"))
            return "Income";
        if (ContainsAny(lower, "rent", "mortgage", "hoa", "insurance"))
            return "Housing";
        if (ContainsAny(lower, "transfer", "payment", "credit card", "loan payment"))
            return "Transfers";
        return "Other";
    }

    private static bool ContainsAny(string source, params string[] terms) =>
        terms.Any(source.Contains);

    private static bool TryParseDate(string value, out DateTimeOffset result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        string[] formats = ["M/d/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "M/d/yy"];
        foreach (string fmt in formats)
        {
            if (DateTimeOffset.TryParseExact(value.Trim(), fmt, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out result))
                return true;
        }
        return DateTimeOffset.TryParse(value.Trim(), CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out result);
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;
        string cleaned = value.Trim().Replace("$", "").Replace(",", "").Replace("(", "-").Replace(")", "");
        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static string SanitizeString(string input) =>
        new string(input.Where(c => !char.IsControl(c)).Take(500).ToArray()).Trim();
}
