using FluentValidation;
using Ledger.Application.Transactions.DTOs;
using Ledger.Core.Interfaces;
using MediatR;

namespace Ledger.Application.Transactions.Commands;

public record UpdateTransactionCategoryCommand(
    Guid TransactionId,
    string Category) : IRequest<TransactionDto>;

public sealed class UpdateTransactionCategoryValidator : AbstractValidator<UpdateTransactionCategoryCommand>
{
    public UpdateTransactionCategoryValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateTransactionCategoryHandler(
    ITransactionRepository transactions,
    IAccountRepository accounts)
    : IRequestHandler<UpdateTransactionCategoryCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(
        UpdateTransactionCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = await transactions.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transaction {request.TransactionId} not found.");

        var account = await accounts.GetByIdAsync(transaction.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Account {transaction.AccountId} not found.");

        transaction.Category = request.Category;
        await transactions.UpdateAsync(transaction, cancellationToken);

        return new TransactionDto(
            transaction.Id,
            transaction.AccountId,
            account.Name,
            transaction.Date,
            transaction.Description,
            transaction.Amount,
            transaction.Category,
            transaction.TransactionType,
            transaction.Notes,
            transaction.ImportBatchId,
            transaction.CreatedAt);
    }
}
