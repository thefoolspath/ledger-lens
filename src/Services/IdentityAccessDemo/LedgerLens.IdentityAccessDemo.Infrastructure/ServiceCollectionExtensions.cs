using LedgerLens.IdentityAccessDemo.Application;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.GetOne;
using LedgerLens.IdentityAccessDemo.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.IdentityAccessDemo.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static TBuilder AddIdentityAccessDemoInfrastructure<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<IdentityAccessDemoDbContext>("identity-demo-db");
        builder.Services.AddScoped<IdentityAccessDemoUserStore>();
        builder.Services.AddScoped<IUsersCreateStore>(services => services.GetRequiredService<IdentityAccessDemoUserStore>());
        builder.Services.AddScoped<IUsersGetOneStore>(services => services.GetRequiredService<IdentityAccessDemoUserStore>());
        return builder;
    }
}
