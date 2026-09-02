using LedgerLens.PortfolioCore.Database;
using LedgerLens.PortfolioCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("portfolio-db")
    ?? throw new InvalidOperationException("ConnectionStrings:portfolio-db is required.");
builder.Services.AddDbContext<PortfolioCoreDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql => npgsql.MigrationsAssembly(typeof(PortfolioCoreDbContextFactory).Assembly.FullName)));

using var host = builder.Build();
await using var scope = host.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioCoreDbContext>();
await dbContext.Database.MigrateAsync();
