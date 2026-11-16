using LedgerLens.Application.Abstractions;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.GetOne;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerLens.IdentityAccessDemo.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityAccessDemoApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ICommandHandler<UsersCreateCommand, UsersCreateResult>, UsersCreateHandler>();
        services.AddScoped<IQueryHandler<UsersGetOneQuery, UserView?>, UsersGetOneHandler>();
        return services;
    }
}
