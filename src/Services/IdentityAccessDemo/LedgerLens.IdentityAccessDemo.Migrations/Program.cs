using LedgerLens.IdentityAccessDemo.Database;
using LedgerLens.IdentityAccessDemo.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("identity-demo-db")
    ?? throw new InvalidOperationException("ConnectionStrings:identity-demo-db is required.");
builder.Services.AddDbContext<IdentityAccessDemoDbContext>(options =>
    options.UseNpgsql(connectionString,
        npgsql => npgsql.MigrationsAssembly(typeof(IdentityAccessDemoDbContextFactory).Assembly.FullName)));
using var host = builder.Build();
await using var scope = host.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDemoDbContext>();
await db.Database.MigrateAsync();
