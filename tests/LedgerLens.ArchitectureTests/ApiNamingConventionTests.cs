using System.Text.Json;
using System.Text.RegularExpressions;
using LedgerLens.ApiContracts;

namespace LedgerLens.ArchitectureTests;

public sealed class ApiNamingConventionTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly (string Route, string OperationKey)[] ExpectedOperations =
    [
        ("/portfolios/create", "PortfoliosCreate"),
        ("/portfolios/get-list", "PortfoliosGetList"),
        ("/portfolios/get-one/{portfolioId:guid}", "PortfoliosGetOne"),
        ("/investment-accounts/create", "InvestmentAccountsCreate"),
        ("/cash-ledger-entries/create", "CashLedgerEntriesCreate"),
        ("/cash-ledger-entries/correct/{entryId:guid}", "CashLedgerEntriesCorrect"),
        ("/simulation-accounts/create", "SimulationAccountsCreate"),
        ("/simulation-accounts/get-list", "SimulationAccountsGetList"),
        ("/simulation-accounts/get-one/{accountId:guid}", "SimulationAccountsGetOne"),
        ("/simulation-trade-drafts/create", "SimulationTradeDraftsCreate"),
        ("/simulation-trade-drafts/confirm/{draftId:guid}", "SimulationTradeDraftsConfirm"),
        ("/simulation-trades/correct/{tradeId:guid}", "SimulationTradesCorrect"),
        ("/simulation-valuations/record-list", "SimulationValuationsRecordList"),
        ("/simulation-valuations/calculate-series-list", "SimulationValuationsCalculateSeriesList"),
        ("/instruments/search-list", "InstrumentsSearchList"),
        ("/quotes/get-latest-list", "QuotesGetLatestList"),
        ("/instrument-candles/get-one/{instrumentId:guid}", "InstrumentCandlesGetOne"),
        ("/foreign-exchange-rates/get-latest-list", "ForeignExchangeRatesGetLatestList"),
    ];

    private static readonly HashSet<string> ApprovedOperations =
    [
        "create", "update", "patch", "delete", "restore", "activate", "deactivate", "confirm", "cancel",
        "correct", "reverse", "create-bulk", "update-bulk", "get-one", "get-list", "get-page", "search-list",
        "search-page", "get-autocomplete-list", "get-combobox-list", "export-file", "record-list",
        "calculate-series-list", "get-latest-list",
    ];

    [Fact]
    public void Implemented_business_routes_have_unique_canonical_operation_keys()
    {
        var source = ApiSource();
        Assert.Equal(ExpectedOperations.Length, ExpectedOperations.Select(item => item.OperationKey).Distinct().Count());
        var discoveredRoutes = Regex.Matches(source, "Map(?:Get|Post|Put|Patch|Delete)\\(\\\"(?<route>/[^\\\"]+)\\\"")
            .Cast<Match>()
            .Select(match => match.Groups["route"].Value)
            .ToArray();
        Assert.Equal(ExpectedOperations.Select(item => item.Route).Order(), discoveredRoutes.Order());

        foreach (var (route, operationKey) in ExpectedOperations)
        {
            Assert.True(IsCanonicalRoute(route), $"Route is not canonical: {route}");
            Assert.Equal(operationKey, DeriveOperationKey(route));
            Assert.Single(Regex.Matches(source, $"WithName\\(\"{Regex.Escape(operationKey)}\"\\)").Cast<Match>());
            Assert.Contains($"\"{route}\"", source, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData("/users/get-one/{userId:guid}", true)]
    [InlineData("/users/roles/get-combobox-list", true)]
    [InlineData("/User/create", false)]
    [InlineData("/user/create", false)]
    [InlineData("/users/get-data", false)]
    [InlineData("/users/list-list", false)]
    [InlineData("/users/save-data", false)]
    public void Route_examples_are_classified_by_the_standard(string route, bool expected) =>
        Assert.Equal(expected, IsCanonicalRoute(route));

    [Fact]
    public void Production_files_have_one_matching_top_level_public_type()
    {
        var sourceRoot = Path.Combine(RepositoryRoot, "src");
        var files = Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file => !ContainsSegment(file, "bin") && !ContainsSegment(file, "obj"))
            .Where(file => !ContainsSegment(file, "Migrations") && !ContainsSegment(file, "Database"))
            .Where(file => !string.Equals(Path.GetFileName(file), "Program.cs", StringComparison.Ordinal));

        foreach (var file in files)
        {
            var matches = Regex.Matches(File.ReadAllText(file),
                @"(?m)^public\s+(?:(?:sealed|static|abstract|partial)\s+)*(?:class|record|struct|enum|interface)\s+(\w+)");
            Assert.True(matches.Count <= 1, $"{file} contains {matches.Count} top-level public types.");
            if (matches.Count == 1)
            {
                Assert.Equal(Path.GetFileNameWithoutExtension(file), matches[0].Groups[1].Value);
            }
        }
    }

    [Fact]
    public void Every_operation_key_is_traceable_through_backend_persistence_and_Angular()
    {
        var api = ApiSource();
        var application = SourceUnder(Path.Combine(RepositoryRoot, "src", "Services"), ".Application");
        var infrastructure = SourceUnder(Path.Combine(RepositoryRoot, "src", "Services"), ".Infrastructure");
        var angular = string.Join('\n', Directory.EnumerateFiles(
            Path.Combine(RepositoryRoot, "web", "ledgerlens-web", "src"), "*.ts", SearchOption.AllDirectories)
            .Select(File.ReadAllText));

        foreach (var (_, operationKey) in ExpectedOperations)
        {
            Assert.Contains(operationKey, api, StringComparison.Ordinal);
            Assert.Contains(operationKey, application, StringComparison.Ordinal);
            Assert.Contains(operationKey, infrastructure, StringComparison.Ordinal);
            Assert.Contains(operationKey, angular, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Collection_contract_fixtures_preserve_required_json_shapes()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var correlationId = Guid.NewGuid().ToString("N");
        var getOne = JsonSerializer.SerializeToElement(
            new ApiResponse<object>(new { id = Guid.CreateVersion7() }, new ApiResponseMeta(correlationId)), options);
        var getList = JsonSerializer.SerializeToElement(
            new ApiResponse<IReadOnlyList<object>>([], new ApiResponseMeta(correlationId)), options);
        var getPage = JsonSerializer.SerializeToElement(
            new ApiResponse<IReadOnlyList<object>>([], new ApiResponseMeta(correlationId,
                new PaginationMetadata(1, 50, 0, 0))), options);

        Assert.Equal(["data", "meta"], getOne.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(JsonValueKind.Object, getOne.GetProperty("data").ValueKind);
        Assert.False(getOne.GetProperty("meta").TryGetProperty("pagination", out _));
        Assert.Equal(JsonValueKind.Array, getList.GetProperty("data").ValueKind);
        Assert.Equal(0, getList.GetProperty("data").GetArrayLength());
        Assert.Equal(["pageNumber", "pageSize", "totalCount", "totalPages"],
            getPage.GetProperty("meta").GetProperty("pagination").EnumerateObject()
                .Select(property => property.Name).ToArray());
    }

    [Fact]
    public void Business_endpoints_use_shared_success_and_Problem_Details_contracts()
    {
        var source = ApiSource();

        Assert.Contains("ApiResults.Ok", source, StringComparison.Ordinal);
        Assert.Contains("ApiProblemFactory", source, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"(?<![A-Za-z0-9_])Results\.(Ok|Created|Conflict)\(", source);
        Assert.DoesNotContain("exception.Message", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new { error =", source, StringComparison.Ordinal);
    }

    private static string ApiSource() => string.Join('\n', Directory.EnumerateFiles(
            Path.Combine(RepositoryRoot, "src", "Services"), "*.cs", SearchOption.AllDirectories)
        .Where(file => file.Contains(".Api", StringComparison.Ordinal))
        .Where(file => !ContainsSegment(file, "bin") && !ContainsSegment(file, "obj"))
        .Select(File.ReadAllText));

    private static string SourceUnder(string root, string projectSuffix) => string.Join('\n',
        Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(file => file.Contains(projectSuffix, StringComparison.Ordinal))
            .Where(file => !ContainsSegment(file, "bin") && !ContainsSegment(file, "obj"))
            .Select(File.ReadAllText));

    private static bool IsCanonicalRoute(string route)
    {
        var segments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2) return false;
        var staticSegments = segments.Where(segment => !segment.StartsWith('{')).ToArray();
        if (staticSegments.Any(segment => segment.Contains('_') || segment.Any(char.IsUpper))) return false;
        if (staticSegments.Any(segment => !Regex.IsMatch(segment, "^[a-z]+(?:-[a-z]+)*$"))) return false;
        return ApprovedOperations.Contains(staticSegments[^1]) && IsPluralFeature(staticSegments[0]);
    }

    private static bool IsPluralFeature(string segment) => segment.EndsWith('s') && segment is not "status";

    private static string DeriveOperationKey(string route) => string.Concat(route
        .Split('/', StringSplitOptions.RemoveEmptyEntries)
        .Where(segment => !segment.StartsWith('{'))
        .SelectMany(segment => segment.Split('-'))
        .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

    private static bool ContainsSegment(string path, string segment) => path.Split(Path.DirectorySeparatorChar)
        .Contains(segment, StringComparer.OrdinalIgnoreCase);

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LedgerLens.slnx"))) return directory.FullName;
        }
        throw new DirectoryNotFoundException("Could not locate the LedgerLens repository root.");
    }

}
