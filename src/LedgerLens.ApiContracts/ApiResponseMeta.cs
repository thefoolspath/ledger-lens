using System.Text.Json.Serialization;

namespace LedgerLens.ApiContracts;

public sealed record ApiResponseMeta(
    string CorrelationId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] PaginationMetadata? Pagination = null);
