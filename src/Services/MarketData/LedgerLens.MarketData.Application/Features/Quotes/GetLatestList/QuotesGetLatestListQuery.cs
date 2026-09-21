using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application.Features.Quotes.GetLatestList;

public sealed record QuotesGetLatestListQuery(IReadOnlyList<MarketInstrument> Instruments);
