namespace LedgerLens.ApiContracts;

public static class ApiErrorCodes
{
    public const string ValidationFailed = "common.validation.failed";
    public const string InvalidIdentifier = "common.validation.invalid_identifier";
    public const string InvalidRequest = "common.validation.invalid_request";
    public const string InvalidLedgerEntryType = "portfolio.cash_ledger_entry.invalid_type";
    public const string InvalidSimulationTradeInput = "portfolio.simulation_trade.invalid_input";
    public const string UnsupportedMediaType = "common.request.unsupported_media_type";
    public const string MutationMarkerMissing = "common.request.mutation_marker_missing";
    public const string LoopbackOriginRequired = "common.request.loopback_origin_required";
    public const string ResourceNotFound = "common.resource.not_found";
    public const string StateConflict = "common.state.conflict";
    public const string UnexpectedFailure = "common.internal.unexpected";
    public const string MarketDataUnavailable = "market.provider.unavailable";
}
