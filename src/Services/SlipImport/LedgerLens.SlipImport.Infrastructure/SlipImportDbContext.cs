using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.SlipImport.Infrastructure;

public sealed class SlipImportDbContext(DbContextOptions<SlipImportDbContext> options) : DbContext(options);

public sealed class SlipImportDbContextFactory : IDesignTimeDbContextFactory<SlipImportDbContext>
{
    public SlipImportDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__slip-db")
            ?? throw new InvalidOperationException("Set ConnectionStrings__slip-db for design-time migrations.");
        var options = new DbContextOptionsBuilder<SlipImportDbContext>().UseNpgsql(connectionString).Options;
        return new SlipImportDbContext(options);
    }
}

public static class SlipImportInfrastructure
{
    public static TBuilder AddSlipImportInfrastructure<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<SlipImportDbContext>("slip-db");
        builder.AddNatsClient("messaging");
        return builder;
    }
}
