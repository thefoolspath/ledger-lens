using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Application.Features.Instruments.SearchList;
using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.UnitTests;

public sealed class InstrumentsSearchListHandlerTests
{
    [Fact]
    public async Task Handler_passes_client_text_without_trimming()
    {
        var provider = new CapturingProvider();
        var handler = new InstrumentsSearchListHandler(provider);

        await handler.HandleAsync(new("  NVDA  ", " US "), CancellationToken.None);

        Assert.Equal("  NVDA  ", provider.Query);
        Assert.Equal(" US ", provider.Market);
    }

    private sealed class CapturingProvider : IMarketDataProvider
    {
        public string? Query { get; private set; }
        public string? Market { get; private set; }

        public Task<IReadOnlyList<MarketInstrument>> InstrumentsSearchListAsync(
            string query,
            string market,
            CancellationToken cancellationToken)
        {
            Query = query;
            Market = market;
            return Task.FromResult<IReadOnlyList<MarketInstrument>>([]);
        }

        public Task<IReadOnlyList<QuoteSnapshot>> QuotesGetLatestListAsync(
            IReadOnlyList<MarketInstrument> instruments,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<CandleSeries?> InstrumentCandlesGetOneAsync(
            Guid instrumentId,
            string interval,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<FxSnapshot>> ForeignExchangeRatesGetLatestListAsync(
            string baseCurrency,
            string quoteCurrency,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
