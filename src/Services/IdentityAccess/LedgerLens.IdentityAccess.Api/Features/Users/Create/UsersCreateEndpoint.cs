using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.IdentityAccess.Application.Features.Users.Create;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.IdentityAccess.Api.Features.Users.Create;

public static class UsersCreateEndpoint
{
    public static RouteHandlerBuilder MapUsersCreate(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/users/create", async (
            UsersCreateRequest request,
            HttpContext context,
            ApiProblemFactory problems,
            ICommandHandler<UsersCreateCommand, UsersCreateResult> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new(request.RequestId, request.Email, request.DisplayName), cancellationToken);
            return result.Failure switch
            {
                UsersCreateFailure.InvalidRequest => problems.Validation(context, ApiErrorCodes.InvalidRequest),
                UsersCreateFailure.EmailAlreadyExists => problems.Conflict(context),
                UsersCreateFailure.RequestIdConflict => problems.Conflict(context),
                null when result.User is not null => ApiResults.Created(context, result.User),
                _ => throw new InvalidOperationException("UsersCreate returned an invalid result."),
            };
        }).WithName("UsersCreate");
}
