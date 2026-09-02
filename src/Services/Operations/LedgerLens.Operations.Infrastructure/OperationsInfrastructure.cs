using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.Operations.Infrastructure;

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
