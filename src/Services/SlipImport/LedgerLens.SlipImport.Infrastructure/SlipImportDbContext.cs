using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.SlipImport.Infrastructure;

public sealed class SlipImportDbContext(DbContextOptions<SlipImportDbContext> options) : DbContext(options);
