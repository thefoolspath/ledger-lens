using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.Operations.Infrastructure;

public sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options) : DbContext(options);
