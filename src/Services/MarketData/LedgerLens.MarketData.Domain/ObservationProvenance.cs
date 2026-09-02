namespace LedgerLens.MarketData.Domain;

public sealed record ObservationProvenance(
    string Provider,
    string Feed,
    DateTimeOffset AsOf,
    DateTimeOffset RetrievedAt,
    FreshnessClass Freshness,
    int? DelaySeconds,
    string RequestId,
    string RetentionPolicyKey);
