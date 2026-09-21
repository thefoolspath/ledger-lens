using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.InvestmentAccounts.Create;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.InvestmentAccounts.Create;

public static class InvestmentAccountsCreateEndpoint
{
    public static RouteHandlerBuilder MapInvestmentAccountsCreate(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/investment-accounts/create", async (
            InvestmentAccountsCreateRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            ICommandHandler<InvestmentAccountsCreateCommand, AccountIdentity?> handler,
            CancellationToken cancellationToken) =>
        {
            if (!PortfolioCoreEndpointValidation.IsVersion7(request.PortfolioId))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "portfolioId");
            try
            {
                var account = await handler.HandleAsync(
                    new(request.PortfolioId, request.Name, request.Broker, request.Currency), cancellationToken);
                return account is null
                    ? problems.NotFound(httpContext)
                    : ApiResults.Created(httpContext, $"/v1/investment-accounts/get-one/{account.Id}", account);
            }
            catch (ArgumentException)
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
            }
        }).WithName("InvestmentAccountsCreate").AddEndpointFilter<LocalMutationFilter>();
}
