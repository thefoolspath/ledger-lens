using Aspire.Hosting.Testing;
using LedgerLens.ApiContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace LedgerLens.DistributedAppTests;

public sealed class FoundationConnectivityTests
{
    private static readonly string[] TestAppHostArguments =
    [
        "--LedgerLens:Runtime:UsePersistentVolumes=false",
        "--Parameters:fixed-user-id=0198f0aa-0000-7000-8000-000000000001",
        "--Parameters:fixed-user-email=owner@example.invalid",
    ];

    [Fact]
    public async Task AppHost_resource_model_can_be_created()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LedgerLens_AppHost>(TestAppHostArguments);
        Assert.NotNull(builder);
    }

    [Fact]
    public async Task Complete_graph_becomes_healthy_and_gateway_reaches_every_service_when_distributed_tests_are_enabled()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LEDGERLENS_RUN_DISTRIBUTED_TESTS"), "1", StringComparison.Ordinal))
        {
            return;
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LedgerLens_AppHost>(TestAppHostArguments, timeout.Token);
        builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
        await using var application = await builder.BuildAsync(timeout.Token);
        await application.StartAsync(timeout.Token);

        var resourceNames = new[]
        {
            "postgres",
            "portfolio-db",
            "market-db",
            "slip-db",
            "research-db",
            "operations-db",
            "messaging",
            "portfolio-api",
            "market-api",
            "slip-api",
            "research-api",
            "operations-api",
            "slip-worker",
            "gateway",
            "web",
        };
        foreach (var resourceName in resourceNames)
        {
            await application.ResourceNotifications.WaitForResourceHealthyAsync(resourceName, timeout.Token);
        }

        using var client = application.CreateHttpClient("gateway");
        var paths = new[]
        {
            "/api/portfolio/internal/ping",
            "/api/market/internal/ping",
            "/api/slips/internal/ping",
            "/api/research/internal/ping",
            "/api/operations/internal/ping",
        };
        foreach (var path in paths)
        {
            using var response = await client.GetAsync(path, timeout.Token);
            response.EnsureSuccessStatusCode();
        }

        using var webClient = application.CreateHttpClient("web");
        using var webResponse = await webClient.GetAsync("/api/portfolio/internal/ping", timeout.Token);
        webResponse.EnsureSuccessStatusCode();
        Assert.Contains("portfolio-api", await webResponse.Content.ReadAsStringAsync(timeout.Token));

        client.DefaultRequestHeaders.Add("Origin", "http://localhost:4200");
        client.DefaultRequestHeaders.Add("X-LedgerLens-Request", "1");
        using var createPortfolioResponse = await client.PostAsJsonAsync(
            "/api/portfolio/v1/portfolios/create",
            new { name = "Synthetic portfolio", baseCurrency = "USD", reportingCurrency = "THB" },
            timeout.Token);
        await EnsureSuccessAsync(createPortfolioResponse, timeout.Token);
        var createdPortfolio = await ReadDataAsync<CreatedResource>(createPortfolioResponse, timeout.Token);
        Assert.Equal(7, createdPortfolio.Id.Version);

        using var createAccountResponse = await client.PostAsJsonAsync(
            "/api/portfolio/v1/investment-accounts/create",
            new { portfolioId = createdPortfolio.Id, name = "Synthetic USD account", broker = "Synthetic Broker", currency = "USD" },
            timeout.Token);
        await EnsureSuccessAsync(createAccountResponse, timeout.Token);
        var createdAccount = await ReadDataAsync<CreatedResource>(createAccountResponse, timeout.Token);
        Assert.Equal(7, createdAccount.Id.Version);

        Guid withdrawalId = Guid.Empty;
        foreach (var entry in new[]
        {
            new { type = "Deposit", amount = 1000m, currency = "USD", effectiveAt = "2026-08-26T00:00:00Z", note = "Synthetic opening cash", instrumentSymbol = (string?)null, quantity = (decimal?)null, unitPrice = (decimal?)null },
            new { type = "Withdrawal", amount = 25m, currency = "USD", effectiveAt = "2026-08-26T01:00:00Z", note = "Synthetic withdrawal", instrumentSymbol = (string?)null, quantity = (decimal?)null, unitPrice = (decimal?)null },
            new { type = "Buy", amount = 200m, currency = "USD", effectiveAt = "2026-08-26T02:00:00Z", note = "Synthetic buy", instrumentSymbol = (string?)"SYNTH", quantity = (decimal?)2m, unitPrice = (decimal?)100m },
            new { type = "Sell", amount = 120m, currency = "USD", effectiveAt = "2026-08-26T03:00:00Z", note = "Synthetic sell", instrumentSymbol = (string?)"SYNTH", quantity = (decimal?)1m, unitPrice = (decimal?)120m },
            new { type = "Fee", amount = 5m, currency = "USD", effectiveAt = "2026-08-26T04:00:00Z", note = "Synthetic fee", instrumentSymbol = (string?)null, quantity = (decimal?)null, unitPrice = (decimal?)null },
            new { type = "Tax", amount = 10m, currency = "USD", effectiveAt = "2026-08-26T05:00:00Z", note = "Synthetic tax", instrumentSymbol = (string?)null, quantity = (decimal?)null, unitPrice = (decimal?)null },
            new { type = "Dividend", amount = 15m, currency = "USD", effectiveAt = "2026-08-26T06:00:00Z", note = "Synthetic dividend", instrumentSymbol = (string?)null, quantity = (decimal?)null, unitPrice = (decimal?)null },
        })
        {
            using var entryResponse = await client.PostAsJsonAsync(
                "/api/portfolio/v1/cash-ledger-entries/create",
                new { accountId = createdAccount.Id, entry.type, entry.amount, entry.currency, entry.effectiveAt, entry.note, entry.instrumentSymbol, entry.quantity, entry.unitPrice },
                timeout.Token);
            await EnsureSuccessAsync(entryResponse, timeout.Token);
            var createdEntry = await ReadDataAsync<CreatedResource>(entryResponse, timeout.Token);
            if (entry.type == "Withdrawal") withdrawalId = createdEntry.Id;
        }

        Assert.Equal(7, withdrawalId.Version);
        var correction = new { type = "Withdrawal", amount = 20m, currency = "USD",
            effectiveAt = "2026-08-26T01:00:00Z", note = "Synthetic corrected withdrawal",
            instrumentSymbol = (string?)null, quantity = (decimal?)null, unitPrice = (decimal?)null,
            reason = "Synthetic amount correction" };
        using var correctionResponse = await client.PostAsJsonAsync(
            $"/api/portfolio/v1/cash-ledger-entries/correct/{withdrawalId}", correction, timeout.Token);
        await EnsureSuccessAsync(correctionResponse, timeout.Token);
        using var duplicateCorrectionResponse = await client.PostAsJsonAsync(
            $"/api/portfolio/v1/cash-ledger-entries/correct/{withdrawalId}", correction, timeout.Token);
        Assert.Equal(System.Net.HttpStatusCode.Conflict, duplicateCorrectionResponse.StatusCode);

        var overview = await GetDataAsync<PortfolioOverview>(client,
            $"/api/portfolio/v1/portfolios/get-one/{createdPortfolio.Id}",
            timeout.Token);
        var account = Assert.Single(overview.Accounts);
        Assert.Equal(900m, account.CashBalance);
        Assert.Equal(9, account.Entries.Length);
        Assert.Equal(2, account.Entries.Count(entry => entry.CorrectsEntryId == withdrawalId));
        Assert.Contains(account.Entries, entry => entry.Role == "Reversal" && entry.SignedAmount == 25m);
        Assert.Contains(account.Entries, entry => entry.Role == "Replacement" && entry.SignedAmount == -20m);

        var instruments = await GetDataAsync<MarketInstrument[]>(client,
            "/api/market/v1/instruments/search-list?query=NVDA&market=US", timeout.Token);
        var instrument = Assert.Single(instruments);
        using var quoteResponse = await client.PostAsJsonAsync("/api/market/v1/quotes/get-latest-list",
            new { instruments = new[] { instrument } }, timeout.Token);
        await EnsureSuccessAsync(quoteResponse, timeout.Token);
        var quote = Assert.Single(await ReadDataAsync<MarketQuote[]>(quoteResponse, timeout.Token));
        using var fxResponse = await client.PostAsJsonAsync("/api/market/v1/foreign-exchange-rates/get-latest-list",
            new { baseCurrency = "USD", quoteCurrency = "THB" }, timeout.Token);
        await EnsureSuccessAsync(fxResponse, timeout.Token);
        var fxRates = await ReadDataAsync<FxRate[]>(fxResponse, timeout.Token);
        var primaryFx = Assert.Single(fxRates, rate => !rate.IsBenchmark);

        using var createSimulationResponse = await client.PostAsJsonAsync(
            "/api/portfolio/v1/simulation-accounts/create",
            new { portfolioId = createdPortfolio.Id, name = "Synthetic NVDA what-if" }, timeout.Token);
        await EnsureSuccessAsync(createSimulationResponse, timeout.Token);
        var simulation = await ReadDataAsync<CreatedResource>(createSimulationResponse, timeout.Token);

        object Evidence() => new
        {
            marketInstrumentId = quote.Instrument.Id,
            quote.Instrument.Symbol,
            quote.Instrument.Exchange,
            quote.Instrument.Mic,
            quote.Instrument.Currency,
            quote.Price,
            quote.PriceKind,
            quote.Provenance.Provider,
            quote.Provenance.Feed,
            quote.Provenance.AsOf,
            quote.Provenance.RetrievedAt,
            freshness = quote.Provenance.Freshness,
            quote.Provenance.DelaySeconds,
            quote.Provenance.RequestId,
            quote.Provenance.RetentionPolicyKey,
        };

        async Task<SimulationTrade> ConfirmAsync(string side, decimal quantity, decimal fee, string effectiveAt)
        {
            using var draftResponse = await client.PostAsJsonAsync(
                "/api/portfolio/v1/simulation-trade-drafts/create",
                new
                {
                    accountId = simulation.Id,
                    side,
                    inputMode = "ByQuantity",
                    requestedQuantity = quantity,
                    requestedAmount = (decimal?)null,
                    assumedFee = fee,
                    assumedTax = 0m,
                    fxRate = primaryFx.Rate,
                    effectiveAt,
                    quote = Evidence(),
                }, timeout.Token);
            await EnsureSuccessAsync(draftResponse, timeout.Token);
            var draft = await ReadDataAsync<SimulationDraft>(draftResponse, timeout.Token);
            using var confirmResponse = await client.PostAsJsonAsync(
                $"/api/portfolio/v1/simulation-trade-drafts/confirm/{draft.Id}",
                new { accountId = simulation.Id }, timeout.Token);
            await EnsureSuccessAsync(confirmResponse, timeout.Token);
            var trade = await ReadDataAsync<SimulationTrade>(confirmResponse, timeout.Token);
            using var duplicateConfirmResponse = await client.PostAsJsonAsync(
                $"/api/portfolio/v1/simulation-trade-drafts/confirm/{draft.Id}",
                new { accountId = simulation.Id }, timeout.Token);
            await EnsureSuccessAsync(duplicateConfirmResponse, timeout.Token);
            var duplicateTrade = await ReadDataAsync<SimulationTrade>(duplicateConfirmResponse, timeout.Token);
            Assert.Equal(trade.Id, duplicateTrade.Id);
            return trade;
        }

        _ = await ConfirmAsync("Buy", 2m, 2m, "2026-08-27T14:00:00Z");
        _ = await ConfirmAsync("Buy", 1m, 1m, "2026-08-28T14:00:00Z");
        var simulatedSell = await ConfirmAsync("Sell", 1.5m, 1m, "2026-08-29T14:00:00Z");
        Assert.Equal("Sell", simulatedSell.Side);

        using var replacementDraftResponse = await client.PostAsJsonAsync(
            "/api/portfolio/v1/simulation-trade-drafts/create",
            new
            {
                accountId = simulation.Id,
                side = "Sell",
                inputMode = "ByQuantity",
                requestedQuantity = 1m,
                requestedAmount = (decimal?)null,
                assumedFee = 1m,
                assumedTax = 0m,
                fxRate = primaryFx.Rate,
                effectiveAt = "2026-08-29T14:00:00Z",
                quote = Evidence(),
            }, timeout.Token);
        await EnsureSuccessAsync(replacementDraftResponse, timeout.Token);
        var replacementDraft = await ReadDataAsync<SimulationDraft>(replacementDraftResponse, timeout.Token);
        using var simulationCorrectionResponse = await client.PostAsJsonAsync(
            $"/api/portfolio/v1/simulation-trades/correct/{simulatedSell.Id}",
            new { replacementDraftId = replacementDraft.Id, reason = "Synthetic quantity correction" }, timeout.Token);
        await EnsureSuccessAsync(simulationCorrectionResponse, timeout.Token);
        var correctionEntries = await ReadDataAsync<SimulationTrade[]>(simulationCorrectionResponse, timeout.Token);
        Assert.Equal(2, correctionEntries.Length);

        var simulationOverview = await GetDataAsync<SimulationOverview>(client,
            $"/api/portfolio/v1/simulation-accounts/get-one/{simulation.Id}", timeout.Token);
        var simulatedPosition = Assert.Single(simulationOverview.Positions);
        Assert.Equal(2m, simulatedPosition.Quantity);
        Assert.Equal(5, simulationOverview.Trades.Length);

        using var valuationResponse = await client.PostAsJsonAsync(
            "/api/portfolio/v1/simulation-valuations/record-list",
            new { accountId = simulation.Id, quotes = new[] { Evidence() }, currentFxRate = primaryFx.Rate }, timeout.Token);
        await EnsureSuccessAsync(valuationResponse, timeout.Token);
        var valuation = Assert.Single(await ReadDataAsync<SimulationValuation[]>(valuationResponse, timeout.Token));
        Assert.True(valuation.IsComplete);
        Assert.Equal(quote.Price * simulatedPosition.Quantity, valuation.CurrentValueUsd);

        var realOverviewAfterSimulation = await GetDataAsync<PortfolioOverview>(client,
            $"/api/portfolio/v1/portfolios/get-one/{createdPortfolio.Id}", timeout.Token);
        var realAccountAfterSimulation = Assert.Single(realOverviewAfterSimulation.Accounts);
        Assert.Equal(900m, realAccountAfterSimulation.CashBalance);
        Assert.Equal(9, realAccountAfterSimulation.Entries.Length);
    }

    private sealed record CreatedResource(Guid Id);

    private sealed record PortfolioOverview(AccountOverview[] Accounts);

    private sealed record AccountOverview(decimal CashBalance, CashEntry[] Entries);

    private sealed record CashEntry(Guid Id, string Role, decimal SignedAmount, Guid? CorrectsEntryId);

    private sealed record MarketInstrument(Guid Id, string Symbol, string Name, string Exchange, string Mic, string Currency);
    private sealed record MarketQuote(MarketInstrument Instrument, decimal Price, string PriceKind, MarketProvenance Provenance);
    private sealed record MarketProvenance(string Provider, string Feed, DateTimeOffset AsOf, DateTimeOffset RetrievedAt,
        string Freshness, int? DelaySeconds, string RequestId, string RetentionPolicyKey);
    private sealed record FxRate(decimal Rate, bool IsBenchmark);
    private sealed record SimulationDraft(Guid Id);
    private sealed record SimulationTrade(Guid Id, string Side);
    private sealed record SimulationOverview(SimulationPosition[] Positions, SimulationTrade[] Trades);
    private sealed record SimulationPosition(string Symbol, decimal Quantity);
    private sealed record SimulationValuation(decimal CurrentValueUsd, bool IsComplete);

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}: {body}");
    }

    private static async Task<T> GetDataAsync<T>(HttpClient client, string path, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadDataAsync<T>(response, cancellationToken);
    }

    private static async Task<T> ReadDataAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken);
        Assert.NotNull(envelope);
        Assert.False(string.IsNullOrWhiteSpace(envelope.Meta.CorrelationId));
        return envelope.Data;
    }
}
