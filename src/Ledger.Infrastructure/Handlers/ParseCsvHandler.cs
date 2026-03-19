using Ledger.Application.Import.Commands;
using Ledger.Application.Import.DTOs;
using Ledger.Infrastructure.Csv;
using MediatR;

namespace Ledger.Infrastructure.Handlers;

public sealed class ParseCsvHandler : IRequestHandler<ParseCsvCommand, ImportPreviewDto>
{
    public async Task<ImportPreviewDto> Handle(ParseCsvCommand request, CancellationToken cancellationToken)
    {
        // Buffer to a MemoryStream so it is seekable (IFormFile streams may not be).
        using var ms = new MemoryStream();
        await request.CsvStream.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;

        // Auto-detect parser from headers; CsvParserFactory resets position after peeking.
        ICsvParser parser = CsvParserFactory.Select(ms);
        ms.Position = 0;

        (IReadOnlyList<ParsedTransactionDto> rows, IReadOnlyList<string> errors) = parser.Parse(ms);

        return new ImportPreviewDto(
            Filename: request.Filename,
            DetectedParser: parser.Name,
            RowCount: rows.Count,
            Rows: rows,
            Errors: errors);
    }
}
