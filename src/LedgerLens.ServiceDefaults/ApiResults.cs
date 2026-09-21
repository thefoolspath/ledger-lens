using LedgerLens.ApiContracts;
using Microsoft.AspNetCore.Http;

namespace LedgerLens.ServiceDefaults;

public static class ApiResults
{
    public static IResult Ok<T>(HttpContext httpContext, T data) =>
        Results.Ok(CreateResponse(httpContext, data));

    public static IResult Created<T>(HttpContext httpContext, string location, T data) =>
        Results.Created(location, CreateResponse(httpContext, data));

    public static IResult Page<T>(HttpContext httpContext, IReadOnlyList<T> data, int pageNumber, int pageSize, long totalCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        var totalPages = totalCount == 0 ? 0 : (totalCount + pageSize - 1) / pageSize;
        var pagination = new PaginationMetadata(pageNumber, pageSize, totalCount, totalPages);
        return Results.Ok(CreateResponse(httpContext, data, pagination));
    }

    private static ApiResponse<T> CreateResponse<T>(HttpContext httpContext, T data, PaginationMetadata? pagination = null) =>
        new(data, new ApiResponseMeta(httpContext.TraceIdentifier, pagination));
}
