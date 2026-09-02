namespace LedgerLens.PortfolioCore.Database.Models;

public sealed class SimulationAccount
{
    public Guid Id { get; set; }
    public Guid PortfolioId { get; set; }
    public string Name { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
