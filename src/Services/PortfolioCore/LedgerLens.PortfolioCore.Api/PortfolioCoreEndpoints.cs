using LedgerLens.ApiContracts;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api;

public static class PortfolioCoreEndpoints
{
    public static IEndpointRouteBuilder MapPortfolioCoreEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/v1");

        group.MapGet("/portfolios/get-list", async (
            HttpContext httpContext,
            PortfoliosGetListHandler handler,
            CancellationToken cancellationToken) =>
            ApiResults.Ok(httpContext, await handler.HandleAsync(cancellationToken)))
            .WithName("PortfoliosGetList");

        group.MapGet("/portfolios/get-one/{portfolioId:guid}", async (
            Guid portfolioId,
            HttpContext httpContext,
            ApiProblemFactory problems,
            PortfoliosGetOneHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(portfolioId))
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "portfolioId");
            }

            var portfolio = await handler.HandleAsync(portfolioId, cancellationToken);
            return portfolio is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, portfolio);
        }).WithName("PortfoliosGetOne");

        group.MapPost("/portfolios/create", async (
            PortfoliosCreateRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            PortfoliosCreateHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var portfolio = await handler.HandleAsync(
                    new PortfoliosCreateCommand(request.Name, request.BaseCurrency, request.ReportingCurrency),
                    cancellationToken);
                return ApiResults.Created(httpContext, $"/v1/portfolios/get-one/{portfolio.Id}", portfolio);
            }
            catch (ArgumentException)
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
            }
        }).WithName("PortfoliosCreate").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/investment-accounts/create", async (
            InvestmentAccountsCreateRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            InvestmentAccountsCreateHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.PortfolioId))
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "portfolioId");
            }

            try
            {
                var account = await handler.HandleAsync(
                    new InvestmentAccountsCreateCommand(request.PortfolioId, request.Name, request.Broker, request.Currency),
                    cancellationToken);
                return account is null
                    ? problems.NotFound(httpContext)
                    : ApiResults.Created(httpContext, $"/v1/investment-accounts/get-one/{account.Id}", account);
            }
            catch (ArgumentException)
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
            }
        }).WithName("InvestmentAccountsCreate").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/cash-ledger-entries/create", async (
            CashLedgerEntriesCreateRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            CashLedgerEntriesCreateHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.AccountId))
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "accountId");
            }

            if (!Enum.TryParse<CashLedgerEntryType>(request.Type, true, out var entryType))
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidLedgerEntryType, "type");
            }

            try
            {
                var entry = await handler.HandleAsync(
                    new CashLedgerEntriesCreateCommand(
                        request.AccountId,
                        entryType,
                        request.Amount,
                        request.Currency,
                        request.EffectiveAt,
                        request.Note,
                        request.InstrumentSymbol,
                        request.Quantity,
                        request.UnitPrice),
                    cancellationToken);
                return entry is null
                    ? problems.NotFound(httpContext)
                    : ApiResults.Created(httpContext, $"/v1/cash-ledger-entries/get-one/{entry.Id}", entry);
            }
            catch (ArgumentException)
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
            }
        }).WithName("CashLedgerEntriesCreate").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/cash-ledger-entries/correct/{entryId:guid}", async (
            Guid entryId,
            CashLedgerEntriesCorrectRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            CashLedgerEntriesCorrectHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(entryId)) return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "entryId");
            if (!Enum.TryParse<CashLedgerEntryType>(request.Type, true, out var entryType))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidLedgerEntryType, "type");
            try
            {
                var result = await handler.HandleAsync(new CashLedgerEntriesCorrectCommand(entryId, entryType, request.Amount,
                    request.Currency, request.EffectiveAt, request.Note, request.InstrumentSymbol, request.Quantity,
                    request.UnitPrice, request.Reason), cancellationToken);
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

        group.MapGet("/simulation-accounts/get-list", async (Guid portfolioId, HttpContext httpContext,
            ApiProblemFactory problems,
            SimulationAccountsGetListHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(portfolioId)) return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "portfolioId");
            return ApiResults.Ok(httpContext, await handler.HandleAsync(portfolioId, cancellationToken));
        }).WithName("SimulationAccountsGetList");

        group.MapPost("/simulation-accounts/create", async (
            SimulationAccountsCreateRequest request, HttpContext httpContext, ApiProblemFactory problems,
            SimulationAccountsCreateHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.PortfolioId)) return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "portfolioId");
            try
            {
                var account = await handler.HandleAsync(new(request.PortfolioId, request.Name), cancellationToken);
                return account is null
                    ? problems.NotFound(httpContext)
                    : ApiResults.Created(httpContext, $"/v1/simulation-accounts/get-one/{account.Id}", account);
            }
            catch (ArgumentException) { return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest); }
        }).WithName("SimulationAccountsCreate").AddEndpointFilter<LocalMutationFilter>();

        group.MapGet("/simulation-accounts/get-one/{accountId:guid}", async (Guid accountId, HttpContext httpContext,
            ApiProblemFactory problems,
            SimulationAccountsGetOneHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(accountId)) return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "accountId");
            var result = await handler.HandleAsync(accountId, cancellationToken);
            return result is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, result);
        }).WithName("SimulationAccountsGetOne");

        group.MapPost("/simulation-trade-drafts/create", async (
            SimulationTradeDraftsCreateRequest request, HttpContext httpContext, ApiProblemFactory problems,
            SimulationTradeDraftsCreateHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.AccountId)) return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "accountId");
            if (!Enum.TryParse<SimulationTradeSide>(request.Side, true, out var side) ||
                !Enum.TryParse<SimulationInputMode>(request.InputMode, true, out var inputMode))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidSimulationTradeInput);
            try
            {
                var result = await handler.HandleAsync(new(request.AccountId, side, inputMode, request.RequestedQuantity,
                    request.RequestedAmount, request.AssumedFee, request.AssumedTax, request.FxRate,
                    request.EffectiveAt, request.Quote), cancellationToken);
                return result is null
                    ? problems.NotFound(httpContext)
                    : ApiResults.Created(httpContext, $"/v1/simulation-trade-drafts/{result.Id}", result);
            }
            catch (ArgumentException) { return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest); }
            catch (InvalidOperationException) { return problems.Conflict(httpContext); }
        }).WithName("SimulationTradeDraftsCreate").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-trade-drafts/confirm/{draftId:guid}", async (
            Guid draftId, SimulationTradeDraftsConfirmRequest request, HttpContext httpContext, ApiProblemFactory problems,
            SimulationTradeDraftsConfirmHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.AccountId) || !IsVersion7(draftId))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier);
            try
            {
                var result = await handler.HandleAsync(request.AccountId, draftId, cancellationToken);
                return result is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, result);
            }
            catch (InvalidOperationException) { return problems.Conflict(httpContext); }
        }).WithName("SimulationTradeDraftsConfirm").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-trades/correct/{tradeId:guid}", async (Guid tradeId,
            SimulationTradesCorrectRequest request, HttpContext httpContext, ApiProblemFactory problems,
            SimulationTradesCorrectHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(tradeId) || !IsVersion7(request.ReplacementDraftId))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier);
            try
            {
                var result = await handler.HandleAsync(new(tradeId, request.ReplacementDraftId, request.Reason), cancellationToken);
                return result is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, result);
            }
            catch (ArgumentException) { return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest); }
            catch (InvalidOperationException) { return problems.Conflict(httpContext); }
        }).WithName("SimulationTradesCorrect").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-valuations/record-list", async (
            SimulationValuationsRecordListRequest request, HttpContext httpContext, ApiProblemFactory problems,
            SimulationValuationsRecordListHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.AccountId)) return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "accountId");
            try
            {
                var result = await handler.HandleAsync(new(request.AccountId, request.Quotes, request.CurrentFxRate), cancellationToken);
                return result is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, result);
            }
            catch (ArgumentException) { return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest); }
            catch (InvalidOperationException) { return problems.Conflict(httpContext); }
        }).WithName("SimulationValuationsRecordList").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-valuations/calculate-series-list", async (
            SimulationValuationsCalculateSeriesListRequest request, HttpContext httpContext, ApiProblemFactory problems,
            SimulationValuationsCalculateSeriesListHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.AccountId)) return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "accountId");
            try
            {
                var result = await handler.HandleAsync(request.AccountId, request.Symbol, request.Observations, cancellationToken);
                return result is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, result);
            }
            catch (ArgumentException) { return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest); }
            catch (InvalidOperationException) { return problems.Conflict(httpContext); }
        }).WithName("SimulationValuationsCalculateSeriesList").AddEndpointFilter<LocalMutationFilter>();

        return endpoints;
    }

    private static bool IsVersion7(Guid id) => id != Guid.Empty && id.Version == 7;
}
