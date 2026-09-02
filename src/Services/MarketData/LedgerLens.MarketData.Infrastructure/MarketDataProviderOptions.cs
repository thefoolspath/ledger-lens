using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Domain;
using Microsoft.Extensions.Options;

namespace LedgerLens.MarketData.Infrastructure;

public sealed class MarketDataProviderOptions
{
    public const string SectionName = "LedgerLens:MarketData";
    public string Provider { get; set; } = "Disabled";
    public bool TermsAccepted { get; set; }
    public string? ApiKey { get; set; }
}
