using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api;

public static class PortfolioCoreEndpoints
{
    public static IEndpointRouteBuilder MapPortfolioCoreEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/v1");

        group.MapGet("/portfolios", async (
            ListPortfoliosHandler handler,
            CancellationToken cancellationToken) =>
            TypedResults.Ok(await handler.HandleAsync(cancellationToken)));

        group.MapGet("/portfolios/{portfolioId:guid}", async (
            Guid portfolioId,
            GetPortfolioOverviewHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(portfolioId))
            {
                return Results.ValidationProblem(InvalidId("portfolioId"));
            }

            var portfolio = await handler.HandleAsync(portfolioId, cancellationToken);
            return portfolio is null ? Results.NotFound() : Results.Ok(portfolio);
        });

        group.MapPost("/portfolios", async (
            CreatePortfolioRequest request,
            CreatePortfolioHandler handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var portfolio = await handler.HandleAsync(
                    new CreatePortfolioCommand(request.Name, request.BaseCurrency, request.ReportingCurrency),
                    cancellationToken);
                return Results.Created($"/v1/portfolios/{portfolio.Id}", portfolio);
            }
            catch (ArgumentException exception)
            {
                return Results.ValidationProblem(InvalidRequest(exception.Message));
            }
        }).AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/portfolios/{portfolioId:guid}/accounts", async (
            Guid portfolioId,
            CreateAccountRequest request,
            CreateAccountHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(portfolioId))
            {
                return Results.ValidationProblem(InvalidId("portfolioId"));
            }

            try
            {
                var account = await handler.HandleAsync(
                    new CreateAccountCommand(portfolioId, request.Name, request.Broker, request.Currency),
                    cancellationToken);
                return account is null
                    ? Results.NotFound()
                    : Results.Created($"/v1/accounts/{account.Id}", account);
            }
            catch (ArgumentException exception)
            {
                return Results.ValidationProblem(InvalidRequest(exception.Message));
            }
        }).AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/accounts/{accountId:guid}/ledger-entries", async (
            Guid accountId,
            RecordLedgerEntryRequest request,
            RecordLedgerEntryHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(accountId))
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
                    new RecordLedgerEntryCommand(
                        accountId,
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
                    : Results.Created($"/v1/ledger-entries/{entry.Id}", entry);
            }
            catch (ArgumentException exception)
            {
                return Results.ValidationProblem(InvalidRequest(exception.Message));
            }
        }).AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/ledger-entries/{entryId:guid}/corrections", async (
            Guid entryId,
            CorrectLedgerEntryRequest request,
            CorrectLedgerEntryHandler handler,
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
                var result = await handler.HandleAsync(new CorrectLedgerEntryCommand(entryId, entryType, request.Amount,
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
        }).AddEndpointFilter<LocalMutationFilter>();

        group.MapGet("/portfolios/{portfolioId:guid}/simulation-accounts", async (Guid portfolioId,
            ListSimulationAccountsHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(portfolioId)) return Results.ValidationProblem(InvalidId("portfolioId"));
            return Results.Ok(await handler.HandleAsync(portfolioId, cancellationToken));
        });

        group.MapPost("/portfolios/{portfolioId:guid}/simulation-accounts", async (Guid portfolioId,
            CreateSimulationAccountRequest request, CreateSimulationAccountHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(portfolioId)) return Results.ValidationProblem(InvalidId("portfolioId"));
            try
            {
                var account = await handler.HandleAsync(new(portfolioId, request.Name), cancellationToken);
                return account is null ? Results.NotFound() : Results.Created($"/v1/simulation-accounts/{account.Id}", account);
            }
            catch (ArgumentException exception) { return Results.ValidationProblem(InvalidRequest(exception.Message)); }
        }).AddEndpointFilter<LocalMutationFilter>();

        group.MapGet("/simulation-accounts/{accountId:guid}", async (Guid accountId,
            GetSimulationOverviewHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(accountId)) return Results.ValidationProblem(InvalidId("accountId"));
            var result = await handler.HandleAsync(accountId, cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPost("/simulation-accounts/{accountId:guid}/trade-drafts", async (Guid accountId,
            CreateSimulationDraftRequest request, CreateSimulationDraftHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(accountId)) return Results.ValidationProblem(InvalidId("accountId"));
            if (!Enum.TryParse<SimulationTradeSide>(request.Side, true, out var side) ||
                !Enum.TryParse<SimulationInputMode>(request.InputMode, true, out var inputMode))
                return Results.ValidationProblem(InvalidRequest("Side must be Buy or Sell and inputMode must be ByQuantity or ByAmount."));
            try
            {
                var result = await handler.HandleAsync(new(accountId, side, inputMode, request.RequestedQuantity,
                    request.RequestedAmount, request.AssumedFee, request.AssumedTax, request.FxRate,
                    request.EffectiveAt, request.Quote), cancellationToken);
                return result is null ? Results.NotFound() : Results.Created($"/v1/simulation-trade-drafts/{result.Id}", result);
            }
            catch (ArgumentException exception) { return Results.ValidationProblem(InvalidRequest(exception.Message)); }
            catch (InvalidOperationException exception) { return Results.Conflict(new { error = exception.Message }); }
        }).AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-accounts/{accountId:guid}/trade-drafts/{draftId:guid}/confirm", async (
            Guid accountId, Guid draftId, ConfirmSimulationDraftHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(accountId) || !IsVersion7(draftId)) return Results.ValidationProblem(InvalidRequest("Account and draft IDs must be UUIDv7 values."));
            try
            {
                var result = await handler.HandleAsync(accountId, draftId, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException exception) { return Results.Conflict(new { error = exception.Message }); }
        }).AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-trades/{tradeId:guid}/corrections", async (Guid tradeId,
            CorrectSimulationTradeRequest request, CorrectSimulationTradeHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(tradeId) || !IsVersion7(request.ReplacementDraftId)) return Results.ValidationProblem(InvalidRequest("Trade and draft IDs must be UUIDv7 values."));
            try
            {
                var result = await handler.HandleAsync(new(tradeId, request.ReplacementDraftId, request.Reason), cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ArgumentException exception) { return Results.ValidationProblem(InvalidRequest(exception.Message)); }
            catch (InvalidOperationException exception) { return Results.Conflict(new { error = exception.Message }); }
        }).AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-accounts/{accountId:guid}/valuations", async (Guid accountId,
            RecordSimulationValuationRequest request, RecordSimulationValuationHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(accountId)) return Results.ValidationProblem(InvalidId("accountId"));
            try
            {
                var result = await handler.HandleAsync(new(accountId, request.Quotes, request.CurrentFxRate), cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ArgumentException exception) { return Results.ValidationProblem(InvalidRequest(exception.Message)); }
            catch (InvalidOperationException exception) { return Results.Conflict(new { error = exception.Message }); }
        }).AddEndpointFilter<LocalMutationFilter>();

        group.MapPost("/simulation-accounts/{accountId:guid}/valuation-series", async (Guid accountId,
            SimulationValuationSeriesRequest request, GetSimulationValuationSeriesHandler handler, CancellationToken cancellationToken) =>
        {
            if (!IsVersion7(accountId)) return Results.ValidationProblem(InvalidId("accountId"));
            try
            {
                var result = await handler.HandleAsync(accountId, request.Symbol, request.Observations, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (ArgumentException exception) { return Results.ValidationProblem(InvalidRequest(exception.Message)); }
            catch (InvalidOperationException exception) { return Results.Conflict(new { error = exception.Message }); }
        }).AddEndpointFilter<LocalMutationFilter>();

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

public sealed record CreatePortfolioRequest(string Name, string BaseCurrency, string ReportingCurrency);

public sealed record CreateAccountRequest(string Name, string Broker, string Currency);

public sealed record RecordLedgerEntryRequest(
    string Type,
    decimal Amount,
    string Currency,
    DateTimeOffset EffectiveAt,
    string? Note,
    string? InstrumentSymbol,
    decimal? Quantity,
    decimal? UnitPrice);

public sealed record CorrectLedgerEntryRequest(
    string Type,
    decimal Amount,
    string Currency,
    DateTimeOffset EffectiveAt,
    string? Note,
    string? InstrumentSymbol,
    decimal? Quantity,
    decimal? UnitPrice,
    string Reason);

public sealed record CreateSimulationAccountRequest(string Name);
public sealed record CreateSimulationDraftRequest(string Side, string InputMode, decimal? RequestedQuantity,
    decimal? RequestedAmount, decimal AssumedFee, decimal AssumedTax, decimal? FxRate, DateTimeOffset EffectiveAt,
    SimulationMarketEvidence Quote);
public sealed record CorrectSimulationTradeRequest(Guid ReplacementDraftId, string Reason);
public sealed record RecordSimulationValuationRequest(IReadOnlyList<SimulationMarketEvidence> Quotes, decimal? CurrentFxRate);
public sealed record SimulationValuationSeriesRequest(string Symbol, IReadOnlyList<SimulationSeriesInput> Observations);

public sealed class LocalMutationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        if (!request.HasJsonContentType())
        {
            return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        if (!request.Headers.TryGetValue("X-LedgerLens-Request", out var marker) || marker != "1")
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Missing local mutation marker.");
        }

        var source = request.Headers.Origin.FirstOrDefault() ?? request.Headers.Referer.FirstOrDefault();
        if (!Uri.TryCreate(source, UriKind.Absolute, out var sourceUri) || !sourceUri.IsLoopback)
        {
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, detail: "Mutation origin must be loopback.");
        }

        return await next(context);
    }
}
