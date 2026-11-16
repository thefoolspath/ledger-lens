using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.IdentityAccessDemo.Application;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.GetOne;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.IdentityAccessDemo.Api.Features.Users.GetOne;

public static class UsersGetOneEndpoint
{
    public static RouteHandlerBuilder MapUsersGetOne(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/users/get-one/{userId:guid}", async (
            Guid userId,
            HttpContext context,
            ApiProblemFactory problems,
            IQueryHandler<UsersGetOneQuery, UserView?> handler,
            CancellationToken cancellationToken) =>
        {
            var user = await handler.HandleAsync(new(userId), cancellationToken);
            return user is null ? problems.NotFound(context) : ApiResults.Ok(context, user);
        }).WithName("UsersGetOne");
}
