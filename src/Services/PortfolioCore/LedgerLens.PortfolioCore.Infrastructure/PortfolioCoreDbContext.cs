using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.PortfolioCore.Infrastructure;

public sealed class PortfolioCoreDbContext(DbContextOptions<PortfolioCoreDbContext> options)
    : DbContext(options);

public sealed class PortfolioCoreDbContextFactory : IDesignTimeDbContextFactory<PortfolioCoreDbContext>
{
    public PortfolioCoreDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__portfolio-db")
            ?? throw new InvalidOperationException("Set ConnectionStrings__portfolio-db for design-time migrations.");
        var options = new DbContextOptionsBuilder<PortfolioCoreDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new PortfolioCoreDbContext(options);
    }
}

public static class PortfolioCoreInfrastructure
{
    public static TBuilder AddPortfolioCoreInfrastructure<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<PortfolioCoreDbContext>("portfolio-db");
        builder.AddNatsClient("messaging");
        return builder;
    }
}
