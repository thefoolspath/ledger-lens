using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.CashLedgerEntries.Create;
using LedgerLens.PortfolioCore.Domain;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.CashLedgerEntries.Create;

public static class CashLedgerEntriesCreateEndpoint
{
    public static RouteHandlerBuilder MapCashLedgerEntriesCreate(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/cash-ledger-entries/create", async (
            CashLedgerEntriesCreateRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            ICommandHandler<CashLedgerEntriesCreateCommand, LedgerEntryView?> handler,
            CancellationToken cancellationToken) =>
        {
            if (!PortfolioCoreEndpointValidation.IsVersion7(request.AccountId))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "accountId");
            if (!Enum.TryParse<CashLedgerEntryType>(request.Type, true, out var entryType))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidLedgerEntryType, "type");
            try
            {
                var entry = await handler.HandleAsync(new(request.AccountId, entryType, request.Amount, request.Currency,
                    request.EffectiveAt, request.Note, request.InstrumentSymbol, request.Quantity, request.UnitPrice),
                    cancellationToken);
                return entry is null
                    ? problems.NotFound(httpContext)
                    : ApiResults.Created(httpContext, entry);
            }
            catch (ArgumentException)
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
            }
        }).WithName("CashLedgerEntriesCreate").AddEndpointFilter<LocalMutationFilter>();
}
