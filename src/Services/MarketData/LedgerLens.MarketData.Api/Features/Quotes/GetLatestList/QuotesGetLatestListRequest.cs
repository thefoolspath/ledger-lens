using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Api.Features.Quotes.GetLatestList;

public sealed record QuotesGetLatestListRequest(IReadOnlyList<MarketInstrument> Instruments);
