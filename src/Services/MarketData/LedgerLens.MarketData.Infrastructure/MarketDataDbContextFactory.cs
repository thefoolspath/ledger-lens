using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LedgerLens.MarketData.Application;

namespace LedgerLens.MarketData.Infrastructure;

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
