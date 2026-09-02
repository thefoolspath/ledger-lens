using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LedgerLens.MarketData.Application;

namespace LedgerLens.MarketData.Infrastructure;

public sealed class MarketDataDbContext(DbContextOptions<MarketDataDbContext> options) : DbContext(options);
