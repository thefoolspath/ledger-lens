namespace LedgerLens.ApiContracts;

public sealed record PaginationMetadata(int PageNumber, int PageSize, long TotalCount, long TotalPages);
