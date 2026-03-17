using Ledger.Application.Import.Commands;
using Ledger.Application.Import.DTOs;
using Ledger.Infrastructure.Csv;
using MediatR;

namespace Ledger.Infrastructure.Handlers;

public sealed class ParseCsvHandler : IRequestHandler<ParseCsvCommand, ImportPreviewDto>
{
    public Task<ImportPreviewDto> Handle(ParseCsvCommand request, CancellationToken cancellationToken)
    {
        var (rows, errors) = GenericCsvParser.Parse(request.CsvStream);

        var preview = new ImportPreviewDto(
            Filename: request.Filename,
            RowCount: rows.Count,
            Rows: rows,
            Errors: errors);

        return Task.FromResult(preview);
    }
}
