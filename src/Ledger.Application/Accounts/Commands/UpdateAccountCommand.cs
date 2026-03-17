using FluentValidation;
using Ledger.Application.Accounts.DTOs;
using Ledger.Core.Interfaces;
using MediatR;

namespace Ledger.Application.Accounts.Commands;

public record UpdateAccountCommand(
    Guid Id,
    string Name,
    string Institution,
    string Currency,
    bool IsActive) : IRequest<AccountDto?>;

public sealed class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Institution).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches("^[A-Z]{3}$");
    }
}

public sealed class UpdateAccountHandler(
    IAccountRepository accounts,
    IBalanceSnapshotRepository snapshots)
    : IRequestHandler<UpdateAccountCommand, AccountDto?>
{
    public async Task<AccountDto?> Handle(
        UpdateAccountCommand request,
        CancellationToken cancellationToken)
    {
        var account = await accounts.GetByIdAsync(request.Id, cancellationToken);
        if (account is null) return null;

        account.Name = request.Name;
        account.Institution = request.Institution;
        account.Currency = request.Currency;
        account.IsActive = request.IsActive;
        account.UpdatedAt = DateTimeOffset.UtcNow;

        await accounts.UpdateAsync(account, cancellationToken);

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
