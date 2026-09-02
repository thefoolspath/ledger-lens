using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.Research.Infrastructure;

public static class ResearchInfrastructure
{
    public static TBuilder AddResearchInfrastructure<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<ResearchDbContext>("research-db");
        builder.AddNatsClient("messaging");
        return builder;
    }
}
