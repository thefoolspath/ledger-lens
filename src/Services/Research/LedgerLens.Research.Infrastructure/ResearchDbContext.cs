using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.Research.Infrastructure;

public sealed class ResearchDbContext(DbContextOptions<ResearchDbContext> options) : DbContext(options);
