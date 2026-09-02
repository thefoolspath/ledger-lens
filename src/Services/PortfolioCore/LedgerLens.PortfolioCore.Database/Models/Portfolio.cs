namespace LedgerLens.PortfolioCore.Database.Models;

public partial class Portfolio
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }

    public string Name { get; set; } = null!;

    public string BaseCurrency { get; set; } = null!;

    public string ReportingCurrency { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
