using FluentValidation;
using Ledger.Application.Accounts.DTOs;
using Ledger.Core.Entities;
using Ledger.Core.Enums;
using Ledger.Core.Interfaces;
using MediatR;

namespace Ledger.Application.Accounts.Commands;

public record CreateAccountCommand(
    string Name,
    string Institution,
    AccountType AccountType,
    string Currency = "USD") : IRequest<AccountDto>;

public sealed class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Institution).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches("^[A-Z]{3}$");
    }
}

public sealed class CreateAccountHandler(IAccountRepository accounts)
    : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Institution = request.Institution,
            AccountType = request.AccountType,
            Currency = request.Currency,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await accounts.AddAsync(account, cancellationToken);

        return new AccountDto(
            created.Id,
            created.Name,
            created.Institution,
            created.AccountType,
            created.Currency,
            created.IsActive,
            null,
            created.CreatedAt,
            created.UpdatedAt);
    }
}
