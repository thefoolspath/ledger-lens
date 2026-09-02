using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Api;

public sealed record QuotesGetLatestListRequest(IReadOnlyList<MarketInstrument> Instruments);
