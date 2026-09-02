namespace LedgerLens.PortfolioCore.Database.Models;

public partial class UserProfile
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
