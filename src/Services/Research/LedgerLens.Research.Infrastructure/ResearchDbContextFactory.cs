using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.Research.Infrastructure;

public sealed class ResearchDbContextFactory : IDesignTimeDbContextFactory<ResearchDbContext>
{
    public ResearchDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__research-db")
            ?? throw new InvalidOperationException("Set ConnectionStrings__research-db for design-time migrations.");
        var options = new DbContextOptionsBuilder<ResearchDbContext>().UseNpgsql(connectionString).Options;
        return new ResearchDbContext(options);
    }
}
