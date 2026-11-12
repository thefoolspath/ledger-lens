using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace LedgerLens.IdentityAccess.Api;

public sealed class UnexpectedIdentityExceptionHandler(
    ILogger<UnexpectedIdentityExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (context.RequestAborted.IsCancellationRequested &&
            exception is OperationCanceledException)
        {
            logger.LogInformation("IdentityAccess request cancelled. Operation={Operation} TraceId={TraceId}",
                context.GetEndpoint()?.DisplayName, Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier);
            context.Response.StatusCode = 499;
            return true;
        }

        logger.LogError("Unexpected IdentityAccess failure. ExceptionType={ExceptionType} Operation={Operation} TraceId={TraceId} CorrelationId={CorrelationId}",
            exception.GetType().FullName,
            context.GetEndpoint()?.DisplayName,
            Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier,
            context.TraceIdentifier);
        await Results.Problem(statusCode: StatusCodes.Status500InternalServerError).ExecuteAsync(context);
        return true;
    }
}
