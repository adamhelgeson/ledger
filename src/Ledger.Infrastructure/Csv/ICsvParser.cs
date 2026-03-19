using Ledger.Application.Import.DTOs;

namespace Ledger.Infrastructure.Csv;

public interface ICsvParser
{
    /// <summary>Human-readable name shown in the import preview UI.</summary>
    string Name { get; }

    /// <summary>
    /// Returns true if this parser recognises the given (lowercase-trimmed) header row.
    /// Parsers are tested in registration order; the last one should be a generic fallback
    /// that always returns true.
    /// </summary>
    bool CanParse(IReadOnlyList<string> headers);

    (IReadOnlyList<ParsedTransactionDto> Rows, IReadOnlyList<string> Errors) Parse(Stream csvStream);
}
