namespace LedgerLens.PortfolioCore.Domain;

public static class DomainId
{
    public static Guid New() => Guid.CreateVersion7();

    public static void RequireVersion7(Guid value, string parameterName)
    {
        if (value == Guid.Empty || value.Version != 7)
        {
            throw new ArgumentException("LedgerLens resource identifiers must be UUIDv7 values.", parameterName);
        }
    }
}
