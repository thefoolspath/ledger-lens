using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application;

public interface IMarketDataProvider
{
    Task<IReadOnlyList<MarketInstrument>> InstrumentsSearchListAsync(string query, string market, CancellationToken cancellationToken);
    Task<IReadOnlyList<QuoteSnapshot>> QuotesGetLatestListAsync(IReadOnlyList<MarketInstrument> instruments, CancellationToken cancellationToken);
    Task<CandleSeries?> InstrumentCandlesGetOneAsync(Guid instrumentId, string interval, DateOnly from, DateOnly to, CancellationToken cancellationToken);
    Task<IReadOnlyList<FxSnapshot>> ForeignExchangeRatesGetLatestListAsync(string baseCurrency, string quoteCurrency, CancellationToken cancellationToken);
}
