using Ledger.Core.Interfaces;
using MediatR;

namespace Ledger.Application.Accounts.Commands;

public record DeleteAccountCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteAccountHandler(IAccountRepository accounts)
    : IRequestHandler<DeleteAccountCommand, bool>
{
    public async Task<bool> Handle(
        DeleteAccountCommand request,
        CancellationToken cancellationToken)
    {
        if (!await accounts.ExistsAsync(request.Id, cancellationToken))
            return false;

        await accounts.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
