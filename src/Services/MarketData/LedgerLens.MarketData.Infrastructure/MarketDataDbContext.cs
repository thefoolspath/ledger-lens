using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.MarketData.Infrastructure;

public sealed class MarketDataDbContext(DbContextOptions<MarketDataDbContext> options) : DbContext(options);

public sealed class MarketDataDbContextFactory : IDesignTimeDbContextFactory<MarketDataDbContext>
{
    public MarketDataDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__market-db")
            ?? throw new InvalidOperationException("Set ConnectionStrings__market-db for design-time migrations.");
        var options = new DbContextOptionsBuilder<MarketDataDbContext>().UseNpgsql(connectionString).Options;
        return new MarketDataDbContext(options);
    }
}

public static class MarketDataInfrastructure
{
    public static TBuilder AddMarketDataInfrastructure<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<MarketDataDbContext>("market-db");
        builder.AddNatsClient("messaging");
        return builder;
    }
}
