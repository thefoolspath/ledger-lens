using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.SlipImport.Infrastructure;

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
