using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application;

public sealed class MarketDataUnavailableException(string message) : Exception(message);
