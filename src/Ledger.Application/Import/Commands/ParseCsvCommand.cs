using Ledger.Application.Import.DTOs;
using MediatR;

namespace Ledger.Application.Import.Commands;

public record ParseCsvCommand(
    Guid AccountId,
    string Filename,
    Stream CsvStream) : IRequest<ImportPreviewDto>;
