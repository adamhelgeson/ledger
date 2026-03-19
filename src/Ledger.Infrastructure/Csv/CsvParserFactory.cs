namespace Ledger.Infrastructure.Csv;

/// <summary>
/// Selects the most appropriate CSV parser by inspecting the header row.
/// Parsers are tested in order; the first match wins.
/// GenericCsvParser is always last because its CanParse always returns true.
/// </summary>
public static class CsvParserFactory
{
    private static readonly ICsvParser[] Parsers =
    [
        new ChaseCsvParser(),
        new GenericCsvParser(),   // Fallback — must be last
    ];

    /// <summary>
    /// Reads the first line of <paramref name="stream"/> (without consuming it),
    /// matches against registered parsers, and returns the best fit.
    /// The caller is responsible for resetting <paramref name="stream"/> to position 0
    /// before calling <see cref="ICsvParser.Parse"/>.
    /// </summary>
    public static ICsvParser Select(Stream stream)
    {
        IReadOnlyList<string> headers = PeekHeaders(stream);
        foreach (ICsvParser parser in Parsers)
        {
            if (parser.CanParse(headers))
                return parser;
        }
        return new GenericCsvParser();
    }

    private static IReadOnlyList<string> PeekHeaders(Stream stream)
    {
        long startPos = stream.Position;
        try
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            string firstLine = reader.ReadLine() ?? string.Empty;

            // Simple comma-split is sufficient for header detection (headers rarely contain commas)
            return firstLine
                .Split(',')
                .Select(h => h.Trim().Trim('"').ToLowerInvariant())
                .ToList()
                .AsReadOnly();
        }
        finally
        {
            if (stream.CanSeek)
                stream.Position = startPos;
        }
    }
}
