namespace LedgerLens.PortfolioCore.Domain;

public static class CurrencyCode
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Currency must be a three-letter ISO 4217 code.", nameof(value));
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || normalized.Any(character => character is < 'A' or > 'Z'))
        {
            throw new ArgumentException("Currency must be a three-letter ISO 4217 code.", nameof(value));
        }

        return normalized;
    }
}
