using CsvHelper;
using CsvHelper.Configuration;
using Ledger.Application.Import.DTOs;
using Ledger.Core.Enums;
using System.Globalization;

namespace Ledger.Infrastructure.Csv;

/// <summary>
/// Generic CSV parser that attempts to auto-detect column mappings.
/// Looks for common column name patterns used by major banks.
/// Extend by adding new column name aliases to the dictionaries below.
/// </summary>
public class GenericCsvParser : ICsvParser
{
    private static readonly string[] DateColumns = ["date", "transaction date", "posted date", "trans date", "post date"];
    private static readonly string[] DescriptionColumns = ["description", "merchant", "payee", "name", "memo", "details", "transaction description"];
    private static readonly string[] AmountColumns = ["amount", "transaction amount", "debit/credit amount"];
    private static readonly string[] DebitColumns = ["debit", "debit amount", "withdrawal", "withdrawals"];
    private static readonly string[] CreditColumns = ["credit", "credit amount", "deposit", "deposits"];

    public string Name => "Generic";

    /// <summary>Always returns true — Generic is the fallback parser.</summary>
    public bool CanParse(IReadOnlyList<string> headers) => true;

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
            TrimOptions = TrimOptions.Trim
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

        var headers = csv.HeaderRecord ?? [];
        var headerMap = headers
            .Select((h, i) => (Header: h.Trim().ToLowerInvariant(), Index: i))
            .ToDictionary(x => x.Header, x => x.Index);

        var dateCol = FindColumn(headerMap, DateColumns);
        var descCol = FindColumn(headerMap, DescriptionColumns);
        var amtCol = FindColumn(headerMap, AmountColumns);
        var debitCol = FindColumn(headerMap, DebitColumns);
        var creditCol = FindColumn(headerMap, CreditColumns);

        if (dateCol is null) errors.Add("Could not identify a date column. Expected one of: " + string.Join(", ", DateColumns));
        if (descCol is null) errors.Add("Could not identify a description column. Expected one of: " + string.Join(", ", DescriptionColumns));
        if (amtCol is null && debitCol is null && creditCol is null)
            errors.Add("Could not identify an amount column. Expected one of: " + string.Join(", ", AmountColumns));

        if (errors.Count > 0) return (rows.AsReadOnly(), errors.AsReadOnly());

        int rowNum = 1;
        while (csv.Read())
        {
            rowNum++;
            try
            {
                var dateStr = dateCol is not null ? csv.GetField(dateCol) ?? "" : "";
                var desc = descCol is not null ? csv.GetField(descCol) ?? "" : "";

                if (!TryParseDate(dateStr, out var date))
                {
                    errors.Add($"Row {rowNum}: could not parse date '{dateStr}'");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(desc))
                {
                    errors.Add($"Row {rowNum}: empty description — skipped");
                    continue;
                }

                // Sanitize description
                desc = SanitizeString(desc);

                decimal amount;
                TransactionType type;

                if (amtCol is not null)
                {
                    // Single amount column — positive = credit, negative = debit (common format)
                    var amtStr = csv.GetField(amtCol) ?? "0";
                    if (!TryParseDecimal(amtStr, out var raw))
                    {
                        errors.Add($"Row {rowNum}: could not parse amount '{amtStr}'");
                        continue;
                    }
                    amount = Math.Abs(raw);
                    type = raw >= 0 ? TransactionType.Credit : TransactionType.Debit;
                }
                else
                {
                    // Separate debit/credit columns
                    var debitStr = debitCol is not null ? csv.GetField(debitCol) ?? "" : "";
                    var creditStr = creditCol is not null ? csv.GetField(creditCol) ?? "" : "";

                    if (!string.IsNullOrWhiteSpace(debitStr) && TryParseDecimal(debitStr, out var debit) && debit != 0)
                    {
                        amount = Math.Abs(debit);
                        type = TransactionType.Debit;
                    }
                    else if (!string.IsNullOrWhiteSpace(creditStr) && TryParseDecimal(creditStr, out var credit) && credit != 0)
                    {
                        amount = Math.Abs(credit);
                        type = TransactionType.Credit;
                    }
                    else
                    {
                        errors.Add($"Row {rowNum}: no valid amount found — skipped");
                        continue;
                    }
                }

                rows.Add(new ParsedTransactionDto(
                    Date: date,
                    Description: desc,
                    Amount: amount,
                    TransactionType: type,
                    Category: CategorizeDescription(desc)));
            }
            catch (Exception ex)
            {
                errors.Add($"Row {rowNum}: unexpected error — {ex.Message}");
            }
        }

        return (rows.AsReadOnly(), errors.AsReadOnly());
    }

    private static string? FindColumn(Dictionary<string, int> headers, string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (headers.TryGetValue(candidate, out var idx))
                return candidate;
        }
        // Partial match fallback
        foreach (var candidate in candidates)
        {
            var match = headers.Keys.FirstOrDefault(h => h.Contains(candidate));
            if (match is not null) return match;
        }
        return null;
    }

    private static bool TryParseDate(string value, out DateTimeOffset result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        string[] formats = ["M/d/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "MM-dd-yyyy", "d/M/yyyy", "dd/MM/yyyy", "M/d/yy"];
        foreach (var fmt in formats)
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
        // Strip currency symbols and thousands separators
        var cleaned = value.Trim().Replace("$", "").Replace(",", "").Replace("(", "-").Replace(")", "");
        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static string SanitizeString(string input) =>
        // Remove control characters, limit length
        new string(input.Where(c => !char.IsControl(c)).Take(500).ToArray()).Trim();

    private static string CategorizeDescription(string desc)
    {
        var lower = desc.ToLowerInvariant();
        if (ContainsAny(lower, "grocery", "whole foods", "trader joe", "kroger", "safeway", "aldi", "costco", "walmart grocery"))
            return "Groceries";
        if (ContainsAny(lower, "restaurant", "cafe", "coffee", "starbucks", "mcdonald", "chipotle", "doordash", "ubereats", "grubhub", "pizza", "sushi", "burger", "taco", "diner"))
            return "Dining";
        if (ContainsAny(lower, "netflix", "spotify", "hulu", "disney", "apple music", "amazon prime", "youtube premium", "github", "adobe", "subscription"))
            return "Subscriptions";
        if (ContainsAny(lower, "shell", "exxon", "chevron", "bp ", "mobil", "gas station", "fuel", "sunoco"))
            return "Gas";
        if (ContainsAny(lower, "electric", "utility", "water bill", "internet", "comcast", "xfinity", "att ", "verizon", "t-mobile", "spectrum"))
            return "Utilities";
        if (ContainsAny(lower, "amazon", "target", "best buy", "apple store", "walmart", "home depot", "lowes", "costco"))
            return "Shopping";
        if (ContainsAny(lower, "cvs", "walgreens", "pharmacy", "doctor", "copay", "medical", "dental", "vision", "gym", "fitness", "planet fitness"))
            return "Health";
        if (ContainsAny(lower, "uber", "lyft", "transit", "parking", "toll", "dmv", "auto", "car wash"))
            return "Transport";
        if (ContainsAny(lower, "direct deposit", "payroll", "salary", "employer", "paycheck", "freelance", "transfer in"))
            return "Income";
        if (ContainsAny(lower, "rent", "mortgage", "hoa", "insurance"))
            return "Housing";
        if (ContainsAny(lower, "transfer", "payment", "credit card", "loan payment"))
            return "Transfers";
        return "Other";
    }

    private static bool ContainsAny(string source, params string[] terms) =>
        terms.Any(source.Contains);
}
