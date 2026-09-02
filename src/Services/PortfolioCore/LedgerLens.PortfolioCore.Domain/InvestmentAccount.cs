namespace LedgerLens.PortfolioCore.Domain;

public sealed class InvestmentAccount
{
    private InvestmentAccount()
    {
    }

    public InvestmentAccount(
        Guid id,
        Guid portfolioId,
        string name,
        string broker,
        string currency,
        DateTimeOffset createdAt)
    {
        DomainId.RequireVersion7(id, nameof(id));
        DomainId.RequireVersion7(portfolioId, nameof(portfolioId));
        Id = id;
        PortfolioId = portfolioId;
        Name = RequireText(name, 100, nameof(name));
        Broker = RequireText(broker, 100, nameof(broker));
        Currency = CurrencyCode.Normalize(currency);
        CreatedAt = createdAt.ToUniversalTime();
    }

    public Guid Id { get; private set; }

    public Guid PortfolioId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Broker { get; private set; } = string.Empty;

    public string Currency { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    private static string RequireText(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} must contain 1 to {maximumLength} characters.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length is < 1 || normalized.Length > maximumLength)
        {
            throw new ArgumentException($"{parameterName} must contain 1 to {maximumLength} characters.", parameterName);
        }

        return normalized;
    }
}
