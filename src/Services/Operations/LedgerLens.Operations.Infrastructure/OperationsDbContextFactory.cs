using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.Operations.Infrastructure;

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
