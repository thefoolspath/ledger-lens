using LedgerLens.Application.Abstractions;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;
using LedgerLens.ServiceDefaults;
using Microsoft.Extensions.Localization;

namespace LedgerLens.IdentityAccessDemo.Api.Features.Users.Create;

public static class UsersCreateEndpoint
{
    public static RouteHandlerBuilder MapUsersCreate(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/users/create", async (
            UsersCreateRequest request,
            HttpContext context,
            ApiProblemFactory problems,
            IStringLocalizer<IdentityDemoMessages> messages,
            ILogger<IdentityDemoMessages> logger,
            ICommandHandler<UsersCreateCommand, UsersCreateResult> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new(request.RequestId, request.Email, request.DisplayName), cancellationToken);
            if (result is { User: null, Failure: { } failure })
                return UsersCreateFailureMapper.Map(failure, context, problems, messages, logger);
            return result is { User: { } user, Failure: null }
                ? ApiResults.Created(context, $"/v1/users/get-one/{user.Id}", user)
                : throw new InvalidOperationException("UsersCreate returned an invalid result.");
        }).WithName("UsersCreate");
}
