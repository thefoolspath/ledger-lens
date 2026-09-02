using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.SlipImport.Infrastructure;

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
