namespace LedgerLens.PortfolioCore.Domain;

public sealed class Portfolio
{
    private Portfolio()
    {
    }

    public Portfolio(
        Guid id,
        Guid ownerUserId,
        string name,
        string baseCurrency,
        string reportingCurrency,
        DateTimeOffset createdAt)
    {
        DomainId.RequireVersion7(id, nameof(id));
        DomainId.RequireVersion7(ownerUserId, nameof(ownerUserId));
        Id = id;
        OwnerUserId = ownerUserId;
        Name = NormalizeName(name);
        BaseCurrency = CurrencyCode.Normalize(baseCurrency);
        ReportingCurrency = CurrencyCode.Normalize(reportingCurrency);
        CreatedAt = createdAt.ToUniversalTime();
    }

    public Guid Id { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string BaseCurrency { get; private set; } = string.Empty;

    public string ReportingCurrency { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Portfolio name must contain 1 to 100 characters.", nameof(name));
        }

        var normalized = name.Trim();
        if (normalized.Length is < 1 or > 100)
        {
            throw new ArgumentException("Portfolio name must contain 1 to 100 characters.", nameof(name));
        }

        return normalized;
    }
}
