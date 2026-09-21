namespace LedgerLens.ApiContracts;

public sealed record ApiResponse<T>(T Data, ApiResponseMeta Meta);
