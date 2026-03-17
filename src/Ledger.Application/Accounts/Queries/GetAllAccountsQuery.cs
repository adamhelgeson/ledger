using Ledger.Application.Accounts.DTOs;
using Ledger.Core.Interfaces;
using MediatR;

namespace Ledger.Application.Accounts.Queries;

public record GetAllAccountsQuery : IRequest<IReadOnlyList<AccountDto>>;

public sealed class GetAllAccountsHandler(
    IAccountRepository accounts,
    IBalanceSnapshotRepository snapshots)
    : IRequestHandler<GetAllAccountsQuery, IReadOnlyList<AccountDto>>
{
    public async Task<IReadOnlyList<AccountDto>> Handle(
        GetAllAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var accountList = await accounts.GetAllAsync(cancellationToken);
        var latestSnapshots = await snapshots.GetAllLatestAsync(cancellationToken);
        var snapshotMap = latestSnapshots.ToDictionary(s => s.AccountId, s => s.Balance);

        return accountList
            .Select(a => new AccountDto(
                a.Id,
                a.Name,
                a.Institution,
                a.AccountType,
                a.Currency,
                a.IsActive,
                snapshotMap.GetValueOrDefault(a.Id),
                a.CreatedAt,
                a.UpdatedAt))
            .ToList()
            .AsReadOnly();
    }
}
