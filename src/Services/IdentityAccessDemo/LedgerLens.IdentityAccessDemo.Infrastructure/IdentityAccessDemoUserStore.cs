using LedgerLens.IdentityAccessDemo.Application;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.GetOne;
using LedgerLens.IdentityAccessDemo.Database;
using LedgerLens.IdentityAccessDemo.Database.Models;
using LedgerLens.IdentityAccessDemo.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LedgerLens.IdentityAccessDemo.Infrastructure;

public sealed class IdentityAccessDemoUserStore(IdentityAccessDemoDbContext db) : IUsersCreateStore, IUsersGetOneStore
{
    public async Task<UsersCreateStoreResult> UsersCreateAsync(User user, Guid requestId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        db.Users.Add(new UserRecord
        {
            Id = user.Id,
            Email = user.Email,
            NormalizedEmail = user.Email.ToUpperInvariant(),
            CreateRequestId = requestId,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt,
        });
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return new(user, null);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation } postgres &&
            postgres.ConstraintName is "IX_users_normalized_email" or "IX_users_create_request_id")
        {
            db.ChangeTracker.Clear();
            var existing = await db.Users.AsNoTracking()
                .SingleOrDefaultAsync(x => x.CreateRequestId == requestId, cancellationToken);
            if (existing is null)
            {
                return new(null, UsersCreateStoreFailure.EmailAlreadyExists);
            }
            return string.Equals(existing.Email, user.Email, StringComparison.Ordinal) &&
                   string.Equals(existing.DisplayName, user.DisplayName, StringComparison.Ordinal)
                ? new(new User(existing.Id, existing.Email, existing.DisplayName, existing.CreatedAt), null)
                : new(null, UsersCreateStoreFailure.RequestIdConflict);
        }
    }

    public async Task<User?> UsersGetOneAsync(Guid userId, CancellationToken cancellationToken)
    {
        var record = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        return record is null ? null : new User(record.Id, record.Email, record.DisplayName, record.CreatedAt);
    }
}
