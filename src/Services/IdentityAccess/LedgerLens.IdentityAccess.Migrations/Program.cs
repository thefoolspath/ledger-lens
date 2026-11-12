using LedgerLens.IdentityAccess.Database;
using LedgerLens.IdentityAccess.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("identity-db")
    ?? throw new InvalidOperationException("ConnectionStrings:identity-db is required.");
builder.Services.AddDbContext<IdentityAccessDbContext>(options =>
    options.UseNpgsql(connectionString,
        npgsql => npgsql.MigrationsAssembly(typeof(IdentityAccessDbContextFactory).Assembly.FullName)));
using var host = builder.Build();
await using var scope = host.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
await db.Database.MigrateAsync();
