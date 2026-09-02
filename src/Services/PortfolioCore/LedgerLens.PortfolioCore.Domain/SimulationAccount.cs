namespace LedgerLens.PortfolioCore.Domain;

public sealed class SimulationAccount
{
    private SimulationAccount() { }

    public SimulationAccount(Guid id, Guid portfolioId, string name, DateTimeOffset createdAt)
    {
        DomainId.RequireVersion7(id, nameof(id));
        DomainId.RequireVersion7(portfolioId, nameof(portfolioId));
        Id = id;
        PortfolioId = portfolioId;
        Name = NormalizeText(name, 100, nameof(name));
        CreatedAt = createdAt.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    internal static string NormalizeText(string? value, int maximumLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", name);
        var normalized = value.Trim();
        if (normalized.Length > maximumLength) throw new ArgumentException($"Value cannot exceed {maximumLength} characters.", name);
        return normalized;
    }
}
