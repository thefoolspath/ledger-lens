namespace LedgerLens.IntegrationContracts;

public sealed record MessageEnvelope<TPayload>(
    Guid MessageId,
    string MessageType,
    int SchemaVersion,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId,
    string? CausationId,
    TPayload Payload);
