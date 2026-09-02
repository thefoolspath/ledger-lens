namespace LedgerLens.PortfolioCore.Database.Models;

public partial class InvestmentAccount
{
    public Guid Id { get; set; }

    public Guid PortfolioId { get; set; }

    public string Name { get; set; } = null!;

    public string Broker { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
