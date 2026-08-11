using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.Operations.Infrastructure;

public sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options) : DbContext(options);

public sealed class OperationsDbContextFactory : IDesignTimeDbContextFactory<OperationsDbContext>
{
    public OperationsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__operations-db")
            ?? throw new InvalidOperationException("Set ConnectionStrings__operations-db for design-time migrations.");
        var options = new DbContextOptionsBuilder<OperationsDbContext>().UseNpgsql(connectionString).Options;
        return new OperationsDbContext(options);
    }
}

public static class OperationsInfrastructure
{
    public static TBuilder AddOperationsInfrastructure<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<OperationsDbContext>("operations-db");
        builder.AddNatsClient("messaging");
        return builder;
    }
}
