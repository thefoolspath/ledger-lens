namespace LedgerLens.PortfolioCore.Domain;

public sealed class UserProfile
{
    private UserProfile()
    {
    }

    public UserProfile(Guid id, string email, DateTimeOffset createdAt)
    {
        DomainId.RequireVersion7(id, nameof(id));
        Id = id;
        Email = NormalizeEmail(email);
        CreatedAt = createdAt.ToUniversalTime();
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public void RefreshEmail(string email) => Email = NormalizeEmail(email);

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("A valid fixed-user email is required.", nameof(email));
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length is < 3 or > 254 || !normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("A valid fixed-user email is required.", nameof(email));
        }

        return normalized;
    }
}
