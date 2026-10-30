namespace LedgerLens.PortfolioCore.Api.Features;

internal static class PortfolioCoreEndpointValidation
{
    internal static bool IsVersion7(Guid id) => id != Guid.Empty && id.Version == 7;
}
