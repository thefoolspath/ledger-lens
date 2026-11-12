using LedgerLens.IdentityAccess.Application;
using LedgerLens.IdentityAccess.Application.Features.Users.Create;
using LedgerLens.IdentityAccess.Application.Features.Users.GetOne;
using LedgerLens.IdentityAccess.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.IdentityAccess.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static TBuilder AddIdentityAccessInfrastructure<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<IdentityAccessDbContext>("identity-db");
        builder.Services.AddScoped<IdentityAccessUserStore>();
        builder.Services.AddScoped<IUsersCreateStore>(services => services.GetRequiredService<IdentityAccessUserStore>());
        builder.Services.AddScoped<IUsersGetOneStore>(services => services.GetRequiredService<IdentityAccessUserStore>());
        return builder;
    }
}
