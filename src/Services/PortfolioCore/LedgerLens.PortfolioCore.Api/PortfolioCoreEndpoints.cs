using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api;

public static class PortfolioCoreEndpoints
{
    public static IEndpointRouteBuilder MapPortfolioCoreEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/v1");

        group.MapGet("/portfolios/get-list", async (
            PortfoliosGetListHandler handler,
            CancellationToken cancellationToken) =>
            TypedResults.Ok(await handler.HandleAsync(cancellationToken)))
            .WithName("PortfoliosGetList");

        group.MapGet("/portfolios/get-one/{portfolioId:guid}", async (
            Guid portfolioId,
            PortfoliosGetOneHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(portfolioId))
            {
                return Results.ValidationProblem(InvalidId("portfolioId"));
            }

            var portfolio = await handler.HandleAsync(portfolioId, cancellationToken);
            return portfolio is null ? Results.NotFound() : Results.Ok(portfolio);
        }).WithName("PortfoliosGetOne");

        group.MapPost("/portfolios/create", async (
            PortfoliosCreateRequest request,
            PortfoliosCreateHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var portfolio = await handler.HandleAsync(
                    new PortfoliosCreateCommand(request.Name, request.BaseCurrency, request.ReportingCurrency),
                    cancellationToken);
                return Results.Created($"/v1/portfolios/get-one/{portfolio.Id}", portfolio);
            }
            catch (ArgumentException exception)
            {
                return Results.ValidationProblem(InvalidRequest(exception.Message));
            }
        }).WithName("PortfoliosCreate").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/investment-accounts/create", async (
            InvestmentAccountsCreateRequest request,
            InvestmentAccountsCreateHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.PortfolioId))
            {
                return Results.ValidationProblem(InvalidId("portfolioId"));
            }

            try
            {
                var account = await handler.HandleAsync(
                    new InvestmentAccountsCreateCommand(request.PortfolioId, request.Name, request.Broker, request.Currency),
                    cancellationToken);
                return account is null
                    ? Results.NotFound()
                    : Results.Created($"/v1/investment-accounts/get-one/{account.Id}", account);
            }
            catch (ArgumentException exception)
            {
                return Results.ValidationProblem(InvalidRequest(exception.Message));
            }
        }).WithName("InvestmentAccountsCreate").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/cash-ledger-entries/create", async (
            CashLedgerEntriesCreateRequest request,
            CashLedgerEntriesCreateHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.AccountId))
            {
                return Results.ValidationProblem(InvalidId("accountId"));
            }

            if (!Enum.TryParse<CashLedgerEntryType>(request.Type, true, out var entryType))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["type"] = ["Type must be Deposit, Withdrawal, Buy, Sell, Fee, Tax, or Dividend."],
                });
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
                    ? Results.NotFound()
                    : Results.Created($"/v1/cash-ledger-entries/get-one/{entry.Id}", entry);
            }
            catch (ArgumentException exception)
            {
                return Results.ValidationProblem(InvalidRequest(exception.Message));
            }
        }).WithName("CashLedgerEntriesCreate").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/cash-ledger-entries/correct/{entryId:guid}", async (
            Guid entryId,
            CashLedgerEntriesCorrectRequest request,
            CashLedgerEntriesCorrectHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(entryId)) return Results.ValidationProblem(InvalidId("entryId"));
            if (!Enum.TryParse<CashLedgerEntryType>(request.Type, true, out var entryType))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["type"] = ["Type must be Deposit, Withdrawal, Buy, Sell, Fee, Tax, or Dividend."],
                });
            try
            {
                var result = await handler.HandleAsync(new CashLedgerEntriesCorrectCommand(entryId, entryType, request.Amount,
                    request.Currency, request.EffectiveAt, request.Note, request.InstrumentSymbol, request.Quantity,
                    request.UnitPrice, request.Reason), cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ArgumentException exception)
            {
                return Results.ValidationProblem(InvalidRequest(exception.Message));
            }
            catch (InvalidOperationException exception)
            {
                return Results.Conflict(new { error = exception.Message });
            }
        }).WithName("CashLedgerEntriesCorrect").AddEndpointFilter<LocalMutationFilter>();

        group.MapGet("/simulation-accounts/get-list", async (Guid portfolioId,
            SimulationAccountsGetListHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(portfolioId)) return Results.ValidationProblem(InvalidId("portfolioId"));
            return Results.Ok(await handler.HandleAsync(portfolioId, cancellationToken));
        }).WithName("SimulationAccountsGetList");

        group.MapPost("/simulation-accounts/create", async (
            SimulationAccountsCreateRequest request, SimulationAccountsCreateHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.PortfolioId)) return Results.ValidationProblem(InvalidId("portfolioId"));
            try
            {
                var account = await handler.HandleAsync(new(request.PortfolioId, request.Name), cancellationToken);
                return account is null ? Results.NotFound() : Results.Created($"/v1/simulation-accounts/get-one/{account.Id}", account);
            }
            catch (ArgumentException exception) { return Results.ValidationProblem(InvalidRequest(exception.Message)); }
        }).WithName("SimulationAccountsCreate").AddEndpointFilter<LocalMutationFilter>();

        group.MapGet("/simulation-accounts/get-one/{accountId:guid}", async (Guid accountId,
            SimulationAccountsGetOneHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(accountId)) return Results.ValidationProblem(InvalidId("accountId"));
            var result = await handler.HandleAsync(accountId, cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName("SimulationAccountsGetOne");

        group.MapPost("/simulation-trade-drafts/create", async (
            SimulationTradeDraftsCreateRequest request, SimulationTradeDraftsCreateHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.AccountId)) return Results.ValidationProblem(InvalidId("accountId"));
            if (!Enum.TryParse<SimulationTradeSide>(request.Side, true, out var side) ||
                !Enum.TryParse<SimulationInputMode>(request.InputMode, true, out var inputMode))
                return Results.ValidationProblem(InvalidRequest("Side must be Buy or Sell and inputMode must be ByQuantity or ByAmount."));
            try
            {
                var result = await handler.HandleAsync(new(request.AccountId, side, inputMode, request.RequestedQuantity,
                    request.RequestedAmount, request.AssumedFee, request.AssumedTax, request.FxRate,
                    request.EffectiveAt, request.Quote), cancellationToken);
                return result is null ? Results.NotFound() : Results.Created($"/v1/simulation-trade-drafts/{result.Id}", result);
            }
            catch (ArgumentException exception) { return Results.ValidationProblem(InvalidRequest(exception.Message)); }
            catch (InvalidOperationException exception) { return Results.Conflict(new { error = exception.Message }); }
        }).WithName("SimulationTradeDraftsCreate").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-trade-drafts/confirm/{draftId:guid}", async (
            Guid draftId, SimulationTradeDraftsConfirmRequest request, SimulationTradeDraftsConfirmHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.AccountId) || !IsVersion7(draftId)) return Results.ValidationProblem(InvalidRequest("Account and draft IDs must be UUIDv7 values."));
            try
            {
                var result = await handler.HandleAsync(request.AccountId, draftId, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException exception) { return Results.Conflict(new { error = exception.Message }); }
        }).WithName("SimulationTradeDraftsConfirm").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-trades/correct/{tradeId:guid}", async (Guid tradeId,
            SimulationTradesCorrectRequest request, SimulationTradesCorrectHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(tradeId) || !IsVersion7(request.ReplacementDraftId)) return Results.ValidationProblem(InvalidRequest("Trade and draft IDs must be UUIDv7 values."));
            try
            {
                var result = await handler.HandleAsync(new(tradeId, request.ReplacementDraftId, request.Reason), cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ArgumentException exception) { return Results.ValidationProblem(InvalidRequest(exception.Message)); }
            catch (InvalidOperationException exception) { return Results.Conflict(new { error = exception.Message }); }
        }).WithName("SimulationTradesCorrect").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-valuations/record-list", async (
            SimulationValuationsRecordListRequest request, SimulationValuationsRecordListHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.AccountId)) return Results.ValidationProblem(InvalidId("accountId"));
            try
            {
                var result = await handler.HandleAsync(new(request.AccountId, request.Quotes, request.CurrentFxRate), cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ArgumentException exception) { return Results.ValidationProblem(InvalidRequest(exception.Message)); }
            catch (InvalidOperationException exception) { return Results.Conflict(new { error = exception.Message }); }
        }).WithName("SimulationValuationsRecordList").AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-valuations/calculate-series-list", async (
            SimulationValuationsCalculateSeriesListRequest request, SimulationValuationsCalculateSeriesListHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(request.AccountId)) return Results.ValidationProblem(InvalidId("accountId"));
            try
            {
                var result = await handler.HandleAsync(request.AccountId, request.Symbol, request.Observations, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ArgumentException exception) { return Results.ValidationProblem(InvalidRequest(exception.Message)); }
            catch (InvalidOperationException exception) { return Results.Conflict(new { error = exception.Message }); }
        }).WithName("SimulationValuationsCalculateSeriesList").AddEndpointFilter<LocalMutationFilter>();

        return endpoints;
    }

    private static bool IsVersion7(Guid id) => id != Guid.Empty && id.Version == 7;

    private static Dictionary<string, string[]> InvalidId(string field) => new()
    {
        [field] = ["LedgerLens resource identifiers must be UUIDv7 values."],
    };

    private static Dictionary<string, string[]> InvalidRequest(string message) => new()
    {
        ["request"] = [message],
    };
}
