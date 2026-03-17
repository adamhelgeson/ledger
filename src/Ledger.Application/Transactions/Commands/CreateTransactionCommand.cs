using FluentValidation;
using Ledger.Application.Transactions.DTOs;
using Ledger.Core.Entities;
using Ledger.Core.Enums;
using Ledger.Core.Interfaces;
using MediatR;

namespace Ledger.Application.Transactions.Commands;

public record CreateTransactionCommand(
    Guid AccountId,
    DateTimeOffset Date,
    string Description,
    decimal Amount,
    string Category,
    TransactionType TransactionType,
    string? Notes = null) : IRequest<TransactionDto>;

public sealed class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}

public sealed class CreateTransactionHandler(
    ITransactionRepository transactions,
    IAccountRepository accounts)
    : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var account = await accounts.GetByIdAsync(request.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Account {request.AccountId} not found.");

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            Date = request.Date,
            Description = request.Description,
            Amount = request.Amount,
            Category = request.Category,
            TransactionType = request.TransactionType,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await transactions.AddAsync(transaction, cancellationToken);

        return new TransactionDto(
            created.Id,
            created.AccountId,
            account.Name,
            created.Date,
            created.Description,
            created.Amount,
            created.Category,
            created.TransactionType,
            created.Notes,
            created.ImportBatchId,
            created.CreatedAt);
    }
}
