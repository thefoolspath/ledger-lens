namespace LedgerLens.IdentityAccess.Domain;

public sealed class User
{
    public User(Guid id, string email, string displayName, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty || id.Version != 7) throw new ArgumentException("A UUIDv7 user ID is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(email) || email.Length > 320)
            throw new ArgumentException("A valid email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 200)
            throw new ArgumentException("A display name is required.", nameof(displayName));
        Id = id;
        Email = email;
        DisplayName = displayName;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public string Email { get; }
    public string DisplayName { get; }
    public DateTimeOffset CreatedAt { get; }
}
