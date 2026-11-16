using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using LedgerLens.ApiContracts;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.IdentityAccessDemo.Api;

public sealed class UnexpectedIdentityExceptionHandler(
    ILogger<UnexpectedIdentityExceptionHandler> logger,
    ApiProblemFactory problems) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (context.RequestAborted.IsCancellationRequested &&
            exception is OperationCanceledException)
        {
            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499;
            return true;
        }

        if (exception is BadHttpRequestException { StatusCode: StatusCodes.Status400BadRequest })
        {
            if (!context.Response.HasStarted)
            {
                context.Response.Headers["X-Correlation-ID"] = context.TraceIdentifier;
                await problems.Validation(context, ApiErrorCodes.InvalidRequest).ExecuteAsync(context);
            }
            return true;
        }

        if (context.Response.HasStarted)
            return false;

        var types = new List<string>(8);
        var current = exception;
        while (current is not null && types.Count < 8)
        {
            types.Add(current.GetType().FullName ?? current.GetType().Name);
            current = current.InnerException;
        }
        var frames = new StackTrace(exception, fNeedFileInfo: false).GetFrames() ?? [];
        var frameNames = frames.Take(64)
            .Select(frame => frame.GetMethod() is { } method
                ? $"{method.DeclaringType?.FullName}.{method.Name}"
                : "unknown")
            .ToArray();
        logger.LogError("Unexpected IdentityAccessDemo failure. ExceptionTypes={ExceptionTypes} TypesTruncated={TypesTruncated} Frames={Frames} FramesTruncated={FramesTruncated} Operation={Operation} TraceId={TraceId} CorrelationId={CorrelationId}",
            string.Join(",", types),
            current is not null,
            string.Join(",", frameNames),
            frames.Length > frameNames.Length,
            OperationName(context),
            Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier,
            context.TraceIdentifier);
        if (!context.RequestAborted.IsCancellationRequested)
        {
            context.Response.Headers["X-Correlation-ID"] = context.TraceIdentifier;
            await Results.Problem(statusCode: StatusCodes.Status500InternalServerError).ExecuteAsync(context);
        }
        return true;
    }

    private static string OperationName(HttpContext context)
    {
        if (context.Request.Path == "/v1/users/create")
            return "UsersCreate";
        if (context.Request.Path.StartsWithSegments("/v1/users/get-one"))
            return "UsersGetOne";
        return "Unknown";
    }
}
