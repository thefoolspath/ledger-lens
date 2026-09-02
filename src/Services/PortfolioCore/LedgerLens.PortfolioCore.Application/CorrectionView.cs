using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record CorrectionView(LedgerEntryView Reversal, LedgerEntryView Replacement);
