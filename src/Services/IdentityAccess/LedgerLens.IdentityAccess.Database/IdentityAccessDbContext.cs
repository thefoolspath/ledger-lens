using LedgerLens.IdentityAccess.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LedgerLens.IdentityAccess.Database;

public sealed class IdentityAccessDbContext(DbContextOptions<IdentityAccessDbContext> options) : DbContext(options)
{
    public DbSet<UserRecord> Users => Set<UserRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var users = modelBuilder.Entity<UserRecord>();
        users.ToTable("users");
        users.HasKey(x => x.Id);
        users.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        users.Property(x => x.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        users.Property(x => x.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(320).IsRequired();
        users.Property(x => x.CreateRequestId).HasColumnName("create_request_id").IsRequired();
        users.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
        users.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        users.HasIndex(x => x.NormalizedEmail).IsUnique();
        users.HasIndex(x => x.CreateRequestId).IsUnique();
    }
}
