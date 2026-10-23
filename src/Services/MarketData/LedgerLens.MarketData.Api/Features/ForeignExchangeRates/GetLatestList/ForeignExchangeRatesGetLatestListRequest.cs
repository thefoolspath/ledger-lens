namespace LedgerLens.MarketData.Api.Features.ForeignExchangeRates.GetLatestList;

public sealed record ForeignExchangeRatesGetLatestListRequest(string BaseCurrency, string QuoteCurrency);
