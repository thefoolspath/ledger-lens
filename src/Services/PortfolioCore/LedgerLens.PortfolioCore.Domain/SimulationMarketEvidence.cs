namespace LedgerLens.PortfolioCore.Domain;

public sealed record SimulationMarketEvidence(
    Guid MarketInstrumentId,
    string Symbol,
    string Exchange,
    string Mic,
    string Currency,
    decimal Price,
    string PriceKind,
    string Provider,
    string Feed,
    DateTimeOffset AsOf,
    DateTimeOffset RetrievedAt,
    string Freshness,
    int? DelaySeconds,
    string RequestId,
    string RetentionPolicyKey)
{
    public SimulationMarketEvidence Normalize()
    {
        DomainId.RequireVersion7(MarketInstrumentId, nameof(MarketInstrumentId));
        RequirePositive(Price, 10, nameof(Price));
        if (!string.Equals(PriceKind, "LastTrade", StringComparison.Ordinal))
            throw new ArgumentException("Simulation trades require a LastTrade observation.", nameof(PriceKind));
        return this with
        {
            Symbol = SymbolValue(Symbol),
            Exchange = SimulationAccount.NormalizeText(Exchange, 80, nameof(Exchange)),
            Mic = SimulationAccount.NormalizeText(Mic, 8, nameof(Mic)).ToUpperInvariant(),
            Currency = CurrencyCode.Normalize(Currency),
            Provider = SimulationAccount.NormalizeText(Provider, 40, nameof(Provider)),
            Feed = SimulationAccount.NormalizeText(Feed, 80, nameof(Feed)),
            Freshness = SimulationAccount.NormalizeText(Freshness, 32, nameof(Freshness)),
            RequestId = SimulationAccount.NormalizeText(RequestId, 100, nameof(RequestId)),
            RetentionPolicyKey = SimulationAccount.NormalizeText(RetentionPolicyKey, 80, nameof(RetentionPolicyKey)),
            AsOf = AsOf.ToUniversalTime(),
            RetrievedAt = RetrievedAt.ToUniversalTime(),
        };
    }

    public static string SymbolValue(string? value)
    {
        var symbol = SimulationAccount.NormalizeText(value, 32, nameof(Symbol)).ToUpperInvariant();
        if (symbol.Any(character => !char.IsLetterOrDigit(character) && character is not '.' and not '-' and not '_'))
            throw new ArgumentException("Instrument symbol contains unsupported characters.", nameof(Symbol));
        return symbol;
    }

    internal static void RequirePositive(decimal value, int scale, string name)
    {
        if (value <= 0 || decimal.Round(value, scale) != value)
            throw new ArgumentOutOfRangeException(name, $"Value must be positive with at most {scale} decimal places.");
    }

    internal static void RequireNonNegative(decimal value, int scale, string name)
    {
        if (value < 0 || decimal.Round(value, scale) != value)
            throw new ArgumentOutOfRangeException(name, $"Value must be non-negative with at most {scale} decimal places.");
    }
}
