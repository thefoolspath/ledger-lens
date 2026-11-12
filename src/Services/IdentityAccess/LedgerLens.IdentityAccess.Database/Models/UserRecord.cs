namespace LedgerLens.IdentityAccess.Database.Models;

public sealed class UserRecord
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string NormalizedEmail { get; set; } = "";
    public Guid CreateRequestId { get; set; }
    public string DisplayName { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}
