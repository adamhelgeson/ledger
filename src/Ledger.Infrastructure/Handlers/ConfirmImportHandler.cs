using Ledger.Application.Import.Commands;
using Ledger.Application.Import.DTOs;
using Ledger.Core.Entities;
using Ledger.Core.Enums;
using Ledger.Core.Interfaces;
using Ledger.Infrastructure.Services;
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
        // ── Deduplication ─────────────────────────────────────────────────────
        // Build a set of existing keys for the account over the import's date range,
        // then filter out rows that are already in the database.
        IReadOnlySet<string> existingKeys = await GetExistingKeysAsync(request, cancellationToken);

        List<ParsedTransactionDto> newRows = request.Rows
            .Where(r => !existingKeys.Contains(MakeKey(r)))
            .ToList();

        int skipped = request.Rows.Count - newRows.Count;

        // If every row is a duplicate, skip batch creation entirely.
        if (newRows.Count == 0)
            return new ConfirmImportDto(Guid.Empty, 0, skipped);

        // ── Create import batch ───────────────────────────────────────────────
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            Filename = request.Filename,
            RowCount = newRows.Count,
            Status = ImportStatus.Pending,
            ImportedAt = now,
        };

        await importBatches.AddAsync(batch, cancellationToken);

        try
        {
            List<Transaction> txs = newRows.Select(row => new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId,
                Date = row.Date,
                Description = row.Description,
                Amount = row.Amount,
                Category = string.IsNullOrWhiteSpace(row.Category)
                    ? TransactionCategorizer.Categorize(row.Description)
                    : row.Category,
                TransactionType = row.TransactionType,
                ImportBatchId = batch.Id,
                CreatedAt = now,
            }).ToList();

            await transactions.AddRangeAsync(txs, cancellationToken);
            await importBatches.UpdateStatusAsync(batch.Id, ImportStatus.Complete, cancellationToken);

            return new ConfirmImportDto(batch.Id, txs.Count, skipped);
        }
        catch
        {
            await importBatches.UpdateStatusAsync(batch.Id, ImportStatus.Error, cancellationToken);
            throw;
        }
    }

    private async Task<IReadOnlySet<string>> GetExistingKeysAsync(
        ConfirmImportCommand request, CancellationToken ct)
    {
        if (request.Rows.Count == 0)
            return new HashSet<string>();

        DateTimeOffset from = request.Rows.Min(r => r.Date);
        DateTimeOffset to = request.Rows.Max(r => r.Date);
        return await transactions.GetDeduplicationKeysAsync(request.AccountId, from, to, ct);
    }

    private static string MakeKey(ParsedTransactionDto row) =>
        $"{row.Date:yyyy-MM-dd}|{row.Amount:F2}|{row.Description.Trim().ToLowerInvariant()}";
}
