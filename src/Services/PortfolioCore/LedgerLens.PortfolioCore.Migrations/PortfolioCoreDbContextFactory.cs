using LedgerLens.PortfolioCore.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LedgerLens.PortfolioCore.Migrations;

public sealed class PortfolioCoreDbContextFactory : IDesignTimeDbContextFactory<PortfolioCoreDbContext>
{
    public PortfolioCoreDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__portfolio-db")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__portfolio-db before running Portfolio Core database tooling.");
        var options = new DbContextOptionsBuilder<PortfolioCoreDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(PortfolioCoreDbContextFactory).Assembly.FullName))
            .Options;
        return new PortfolioCoreDbContext(options);
    }
}
