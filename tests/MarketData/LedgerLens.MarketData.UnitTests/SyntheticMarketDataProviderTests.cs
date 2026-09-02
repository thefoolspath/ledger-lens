using LedgerLens.MarketData.Domain;
using LedgerLens.MarketData.Infrastructure;

namespace LedgerLens.MarketData.UnitTests;

public sealed class SyntheticMarketDataProviderTests
{
    [Fact]
    public async Task Nvda_search_quote_candles_and_fx_are_provenance_labelled()
    {
        var provider = new SyntheticMarketDataProvider(new FixedTimeProvider(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero)));
        var instrument = Assert.Single(await provider.SearchAsync("NVDA", "US", CancellationToken.None));
        var quote = Assert.Single(await provider.GetLatestQuotesAsync([instrument], CancellationToken.None));
        var candles = await provider.GetCandlesAsync(instrument.Id, "1day", new DateOnly(2026, 8, 1), new DateOnly(2026, 9, 1), CancellationToken.None);
        var fx = await provider.GetFxAsync("USD", "THB", CancellationToken.None);

        Assert.Equal(7, instrument.Id.Version);
        Assert.Equal("LastTrade", quote.PriceKind);
        Assert.Equal(FreshnessClass.Synthetic, quote.Provenance.Freshness);
        Assert.NotNull(candles);
        Assert.NotEmpty(candles.Candles);
        Assert.Equal(2, fx.Count);
        Assert.Contains(fx, rate => rate.IsBenchmark);
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
