using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application.Features.InvestmentAccounts.Create;

public sealed class InvestmentAccountsCreateHandler(ICurrentUser currentUser, IPortfolioCoreStore store, TimeProvider timeProvider)
    : ICommandHandler<InvestmentAccountsCreateCommand, AccountIdentity?>
{
    public async Task<AccountIdentity?> HandleAsync(InvestmentAccountsCreateCommand command, CancellationToken cancellationToken)
    {
        if (!await store.PortfolioIsOwnedByAsync(command.PortfolioId, currentUser.UserId, cancellationToken)) return null;
        var account = new InvestmentAccount(DomainId.New(), command.PortfolioId, command.Name, command.Broker, command.Currency, timeProvider.GetUtcNow());
        await store.InvestmentAccountsCreateAsync(account, cancellationToken);
        return new(account.Id, account.PortfolioId, account.Currency);
    }
}
