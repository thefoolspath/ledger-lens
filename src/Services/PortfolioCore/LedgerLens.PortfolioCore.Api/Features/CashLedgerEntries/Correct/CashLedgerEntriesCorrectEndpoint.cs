using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.CashLedgerEntries.Correct;
using LedgerLens.PortfolioCore.Domain;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.CashLedgerEntries.Correct;

public static class CashLedgerEntriesCorrectEndpoint
{
    public static RouteHandlerBuilder MapCashLedgerEntriesCorrect(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/cash-ledger-entries/correct/{entryId:guid}", async (
            Guid entryId,
            CashLedgerEntriesCorrectRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            ICommandHandler<CashLedgerEntriesCorrectCommand, CorrectionView?> handler,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<CashLedgerEntryType>(request.Type, true, out var entryType))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidLedgerEntryType, "type");
            try
            {
                var result = await handler.HandleAsync(new(entryId, entryType, request.Amount, request.Currency,
                    request.EffectiveAt, request.Note, request.InstrumentSymbol, request.Quantity, request.UnitPrice,
                    request.Reason), cancellationToken);
                return result is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, result);
            }
            catch (ArgumentException)
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
            }
            catch (InvalidOperationException)
            {
                return problems.Conflict(httpContext);
            }
        }).WithName("CashLedgerEntriesCorrect").AddEndpointFilter<LocalMutationFilter>();
}
