using LedgerLens.Application.Abstractions;
using LedgerLens.IdentityAccess.Application.Features.Users.Create;
using LedgerLens.IdentityAccess.Application.Features.Users.GetOne;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerLens.IdentityAccess.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityAccessApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ICommandHandler<UsersCreateCommand, UsersCreateResult>, UsersCreateHandler>();
        services.AddScoped<IQueryHandler<UsersGetOneQuery, UserView?>, UsersGetOneHandler>();
        return services;
    }
}
