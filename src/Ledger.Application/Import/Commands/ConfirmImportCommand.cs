using Ledger.Application.Import.DTOs;
using MediatR;

namespace Ledger.Application.Import.Commands;

public record ConfirmImportCommand(
    Guid AccountId,
    string Filename,
    IReadOnlyList<ParsedTransactionDto> Rows) : IRequest<ConfirmImportDto>;
