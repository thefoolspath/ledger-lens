using LedgerLens.IdentityAccess.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LedgerLens.IdentityAccess.Migrations;

public sealed class IdentityAccessDbContextFactory : IDesignTimeDbContextFactory<IdentityAccessDbContext>
{
    public IdentityAccessDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__identity-db")
            ?? throw new InvalidOperationException("Set ConnectionStrings__identity-db before running IdentityAccess database tooling.");
        var options = new DbContextOptionsBuilder<IdentityAccessDbContext>()
            .UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(IdentityAccessDbContextFactory).Assembly.FullName))
            .Options;
        return new IdentityAccessDbContext(options);
    }
}
