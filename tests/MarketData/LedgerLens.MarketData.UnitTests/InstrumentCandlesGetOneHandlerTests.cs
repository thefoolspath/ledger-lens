using LedgerLens.MarketData.Application.Features.InstrumentCandles.GetOne;
using LedgerLens.MarketData.Infrastructure;

namespace LedgerLens.MarketData.UnitTests;

public sealed class InstrumentCandlesGetOneHandlerTests
{
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("019a0f10-0000-4000-8000-000000000001")]
    public async Task Unknown_guid_returns_not_found_without_reaching_a_candle(string id)
    {
        var handler = new InstrumentCandlesGetOneHandler(new SyntheticMarketDataProvider(TimeProvider.System));

        var result = await handler.HandleAsync(
            new(Guid.Parse(id), "1day", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 2)),
            CancellationToken.None);

        Assert.Null(result);
    }
}
