using Ledger.Application.Import.Commands;
using Ledger.Application.Import.DTOs;
using Ledger.Core.Entities;
using Ledger.Core.Enums;
using Ledger.Core.Interfaces;
using MediatR;

namespace Ledger.Infrastructure.Handlers;

public sealed class ConfirmImportHandler(
    IImportBatchRepository importBatches,
    ITransactionRepository transactions)
    : IRequestHandler<ConfirmImportCommand, ConfirmImportDto>
{
    public async Task<ConfirmImportDto> Handle(
        ConfirmImportCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            Filename = request.Filename,
            RowCount = request.Rows.Count,
            Status = ImportStatus.Pending,
            ImportedAt = now
        };

        await importBatches.AddAsync(batch, cancellationToken);

        try
        {
            var txs = request.Rows.Select(row => new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId,
                Date = row.Date,
                Description = row.Description,
                Amount = row.Amount,
                Category = row.Category,
                TransactionType = row.TransactionType,
                ImportBatchId = batch.Id,
                CreatedAt = now
            }).ToList();

            await transactions.AddRangeAsync(txs, cancellationToken);
            await importBatches.UpdateStatusAsync(batch.Id, ImportStatus.Complete, cancellationToken);

            return new ConfirmImportDto(batch.Id, txs.Count);
        }
        catch
        {
            await importBatches.UpdateStatusAsync(batch.Id, ImportStatus.Error, cancellationToken);
            throw;
        }
    }
}
