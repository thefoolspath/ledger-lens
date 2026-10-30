namespace LedgerLens.MarketData.Application.Features.ForeignExchangeRates.GetLatestList;

public sealed record ForeignExchangeRatesGetLatestListQuery(string BaseCurrency, string QuoteCurrency);
