using Ledger.Application.Common;
using Ledger.Application.Transactions.DTOs;
using Ledger.Core.Interfaces;
using MediatR;

namespace Ledger.Application.Transactions.Queries;

public record GetTransactionsQuery(TransactionFilterDto Filter) : IRequest<PaginatedResult<TransactionDto>>;

public sealed class GetTransactionsHandler(ITransactionRepository transactions)
    : IRequestHandler<GetTransactionsQuery, PaginatedResult<TransactionDto>>
{
    public async Task<PaginatedResult<TransactionDto>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var f = request.Filter;
        var (items, total) = await transactions.GetPagedAsync(
            f.AccountId, f.Category, f.From, toDate: f.To, f.Search,
            f.Page, f.PageSize, cancellationToken);

        var dtos = items
            .Select(t => new TransactionDto(
                t.Id,
                t.AccountId,
                t.Account.Name,
                t.Date,
                t.Description,
                t.Amount,
                t.Category,
                t.TransactionType,
                t.Notes,
                t.ImportBatchId,
                t.CreatedAt))
            .ToList()
            .AsReadOnly();

        return PaginatedResult<TransactionDto>.Create(dtos, total, f.Page, f.PageSize);
    }
}
