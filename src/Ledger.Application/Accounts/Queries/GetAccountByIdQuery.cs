using Ledger.Application.Accounts.DTOs;
using Ledger.Core.Interfaces;
using MediatR;

namespace Ledger.Application.Accounts.Queries;

public record GetAccountByIdQuery(Guid Id) : IRequest<AccountDto?>;

public sealed class GetAccountByIdHandler(
    IAccountRepository accounts,
    IBalanceSnapshotRepository snapshots)
    : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    public async Task<AccountDto?> Handle(
        GetAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var account = await accounts.GetByIdAsync(request.Id, cancellationToken);
        if (account is null) return null;

        var snapshot = await snapshots.GetLatestAsync(account.Id, cancellationToken);

        return new AccountDto(
            account.Id,
            account.Name,
            account.Institution,
            account.AccountType,
            account.Currency,
            account.IsActive,
            snapshot?.Balance,
            account.CreatedAt,
            account.UpdatedAt);
    }
}
