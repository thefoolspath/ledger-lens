using System.Diagnostics;
using System.Globalization;
using LedgerLens.ApiContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LedgerLens.ServiceDefaults;

public sealed class ApiProblemFactory(
    IStringLocalizer<SharedApiMessages> messages,
    ILogger<ApiProblemFactory> logger)
{
    public IResult Validation(HttpContext httpContext, string code, string field = "request")
    {
        var problem = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            [field] = [GetText(code, "Detail")],
        })
        {
            Type = "about:blank",
            Title = GetText(code, "Title"),
            Status = StatusCodes.Status400BadRequest,
            Detail = GetText(ApiErrorCodes.ValidationFailed, "Detail"),
            Instance = httpContext.Request.Path,
        };
        AddDiagnostics(problem, httpContext, code);
        return Results.Json(problem, statusCode: problem.Status, contentType: "application/problem+json");
    }

    public IResult NotFound(HttpContext httpContext) =>
        Problem(httpContext, StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound);

    public IResult Conflict(HttpContext httpContext) =>
        Problem(httpContext, StatusCodes.Status409Conflict, ApiErrorCodes.StateConflict);

    public IResult UnsupportedMediaType(HttpContext httpContext) =>
        Problem(httpContext, StatusCodes.Status415UnsupportedMediaType, ApiErrorCodes.UnsupportedMediaType);

    public IResult BadRequest(HttpContext httpContext, string code) =>
        Problem(httpContext, StatusCodes.Status400BadRequest, code);

    public IResult Forbidden(HttpContext httpContext, string code) =>
        Problem(httpContext, StatusCodes.Status403Forbidden, code);

    public IResult ServiceUnavailable(HttpContext httpContext, string code) =>
        Problem(httpContext, StatusCodes.Status503ServiceUnavailable, code);

    internal void Customize(ProblemDetails problem, HttpContext httpContext)
    {
        var status = problem.Status ?? StatusCodes.Status500InternalServerError;
        var code = problem.Extensions.TryGetValue("code", out var existingCode)
            ? existingCode?.ToString() ?? CodeForStatus(status)
            : CodeForStatus(status);

        problem.Status = status;
        problem.Type ??= "about:blank";
        problem.Instance ??= httpContext.Request.Path;
        problem.Title = GetText(code, "Title");
        problem.Detail = GetText(code, "Detail");
        AddDiagnostics(problem, httpContext, code);
    }

    private IResult Problem(HttpContext httpContext, int status, string code)
    {
        var problem = new ProblemDetails
        {
            Type = "about:blank",
            Title = GetText(code, "Title"),
            Status = status,
            Detail = GetText(code, "Detail"),
            Instance = httpContext.Request.Path,
        };
        AddDiagnostics(problem, httpContext, code);
        return Results.Json(problem, statusCode: status, contentType: "application/problem+json");
    }

    private string GetText(string code, string member)
    {
        var localized = messages[$"{code}.{member}"];
        if (!localized.ResourceNotFound)
        {
            return localized.Value;
        }

        logger.LogWarning("API message resource {ResourceKey} was not found for culture {Culture}",
            localized.Name, CultureInfo.CurrentUICulture.Name);
        var fallback = messages[$"{ApiErrorCodes.UnexpectedFailure}.{member}"];
        return fallback.ResourceNotFound ? "The request could not be completed." : fallback.Value;
    }

    private static void AddDiagnostics(ProblemDetails problem, HttpContext httpContext, string code)
    {
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;
    }

    private static string CodeForStatus(int status) => status switch
    {
        StatusCodes.Status400BadRequest => ApiErrorCodes.ValidationFailed,
        StatusCodes.Status404NotFound => ApiErrorCodes.ResourceNotFound,
        StatusCodes.Status409Conflict => ApiErrorCodes.StateConflict,
        StatusCodes.Status415UnsupportedMediaType => ApiErrorCodes.UnsupportedMediaType,
        StatusCodes.Status503ServiceUnavailable => ApiErrorCodes.MarketDataUnavailable,
        _ => ApiErrorCodes.UnexpectedFailure,
    };
}
