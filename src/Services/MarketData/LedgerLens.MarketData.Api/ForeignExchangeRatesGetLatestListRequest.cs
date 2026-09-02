using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Api;

public sealed record ForeignExchangeRatesGetLatestListRequest(string BaseCurrency, string QuoteCurrency);
