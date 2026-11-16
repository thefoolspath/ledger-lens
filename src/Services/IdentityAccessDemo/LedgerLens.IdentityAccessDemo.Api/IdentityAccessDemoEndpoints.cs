using LedgerLens.IdentityAccessDemo.Api.Features.Users.Create;
using LedgerLens.IdentityAccessDemo.Api.Features.Users.GetOne;

namespace LedgerLens.IdentityAccessDemo.Api;

public static class IdentityAccessDemoEndpoints
{
    public static IEndpointRouteBuilder MapIdentityAccessDemoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var users = endpoints.MapGroup("/v1");
        users.MapUsersCreate();
        users.MapUsersGetOne();
        return endpoints;
    }
}
