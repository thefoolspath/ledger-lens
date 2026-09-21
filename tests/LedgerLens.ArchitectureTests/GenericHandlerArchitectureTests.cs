using System.Text.RegularExpressions;

namespace LedgerLens.ArchitectureTests;

public sealed class GenericHandlerArchitectureTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void Every_business_handler_implements_one_generic_contract_and_is_registered_explicitly()
    {
        var applicationRoot = Path.Combine(RepositoryRoot, "src", "Services");
        var handlerFiles = Directory.EnumerateFiles(applicationRoot, "*Handler.cs", SearchOption.AllDirectories)
            .Where(file => file.Contains(".Application", StringComparison.Ordinal))
            .Where(file => !ContainsSegment(file, "bin") && !ContainsSegment(file, "obj"))
            .Order()
            .ToArray();
        var registrations = string.Join('\n', Directory.EnumerateFiles(applicationRoot, "ServiceCollectionExtensions.cs", SearchOption.AllDirectories)
            .Where(file => file.Contains(".Application", StringComparison.Ordinal))
            .Select(File.ReadAllText));

        Assert.Equal(18, handlerFiles.Length);
        foreach (var file in handlerFiles)
        {
            var source = File.ReadAllText(file);
            var handlerName = Path.GetFileNameWithoutExtension(file);
            var contractCount = Regex.Matches(source, @":\s*I(?:Command|Query)Handler<").Count;

            Assert.Equal(1, contractCount);
            Assert.Contains($", {handlerName}>();", registrations, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Endpoints_inject_generic_contracts_without_dispatch_or_reflection_registration()
    {
        var sourceRoot = Path.Combine(RepositoryRoot, "src");
        var apiSource = SourceUnder(sourceRoot, ".Api");
        var applicationSource = SourceUnder(sourceRoot, ".Application");

        Assert.Equal(18, Regex.Matches(apiSource, @"I(?:Command|Query)Handler<").Count);
        Assert.DoesNotMatch(@"\b[A-Za-z0-9]+Handler\s+handler\b", apiSource);
        Assert.DoesNotContain("IMediator", applicationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("IServiceProvider", applicationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Assembly.GetTypes", applicationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("services.Scan", applicationSource, StringComparison.Ordinal);
    }

    private static string SourceUnder(string root, string projectSuffix) => string.Join('\n',
        Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(file => file.Contains(projectSuffix, StringComparison.Ordinal))
            .Where(file => !ContainsSegment(file, "bin") && !ContainsSegment(file, "obj"))
            .Select(File.ReadAllText));

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
