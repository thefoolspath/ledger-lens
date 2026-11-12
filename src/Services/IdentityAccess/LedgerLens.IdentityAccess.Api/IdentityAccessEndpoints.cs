using LedgerLens.IdentityAccess.Api.Features.Users.Create;
using LedgerLens.IdentityAccess.Api.Features.Users.GetOne;

namespace LedgerLens.IdentityAccess.Api;

public static class IdentityAccessEndpoints
{
    public static IEndpointRouteBuilder MapIdentityAccessEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var users = endpoints.MapGroup("/v1");
        users.MapUsersCreate();
        users.MapUsersGetOne();
        return endpoints;
    }
}
