using LedgerLens.IdentityAccessDemo.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LedgerLens.IdentityAccessDemo.Migrations;

public sealed class IdentityAccessDemoDbContextFactory : IDesignTimeDbContextFactory<IdentityAccessDemoDbContext>
{
    public IdentityAccessDemoDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__identity-demo-db")
            ?? throw new InvalidOperationException("Set ConnectionStrings__identity-demo-db before running IdentityAccessDemo database tooling.");
        var options = new DbContextOptionsBuilder<IdentityAccessDemoDbContext>()
            .UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(IdentityAccessDemoDbContextFactory).Assembly.FullName))
            .Options;
        return new IdentityAccessDemoDbContext(options);
    }
}
