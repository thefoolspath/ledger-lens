using System.Xml.Linq;

namespace LedgerLens.ArchitectureTests;

public sealed class ServiceBoundaryTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string ServicesRoot = Path.Combine(RepositoryRoot, "src", "Services");

    [Fact]
    public void Domain_projects_are_dependency_free()
    {
        var domainProjects = Directory.EnumerateFiles(ServicesRoot, "*.Domain.csproj", SearchOption.AllDirectories);
        foreach (var project in domainProjects)
        {
            var document = XDocument.Load(project);
            Assert.Empty(document.Descendants("PackageReference"));
            Assert.Empty(document.Descendants("ProjectReference"));
        }
    }

    [Fact]
    public void Service_projects_do_not_reference_another_service()
    {
        foreach (var project in Directory.EnumerateFiles(ServicesRoot, "*.csproj", SearchOption.AllDirectories))
        {
            var owner = GetServiceName(project);
            var document = XDocument.Load(project);
            foreach (var reference in document.Descendants("ProjectReference"))
            {
                var include = Assert.IsType<XAttribute>(reference.Attribute("Include")).Value;
                var referencedPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(project)!, include));
                var referencedService = GetServiceName(referencedPath);
                if (referencedService is not null)
                {
                    Assert.Equal(owner, referencedService);
                }
            }
        }
    }

    [Fact]
    public void Application_and_infrastructure_dependencies_point_inward()
    {
        foreach (var project in Directory.EnumerateFiles(ServicesRoot, "*.csproj", SearchOption.AllDirectories))
        {
            var projectName = Path.GetFileNameWithoutExtension(project);
            var serviceReferences = XDocument.Load(project)
                .Descendants("ProjectReference")
                .Select(reference => reference.Attribute("Include")!.Value)
                .Where(include => include.Contains("LedgerLens.", StringComparison.Ordinal))
                .Select(Path.GetFileNameWithoutExtension)
                .Where(reference => reference is not null)
                .ToArray();

            if (projectName.EndsWith(".Application", StringComparison.Ordinal))
            {
                Assert.All(serviceReferences, reference => Assert.EndsWith(".Domain", reference, StringComparison.Ordinal));
            }
            else if (projectName.EndsWith(".Infrastructure", StringComparison.Ordinal))
            {
                Assert.DoesNotContain(serviceReferences, reference => reference!.EndsWith(".Api", StringComparison.Ordinal));
                Assert.DoesNotContain(serviceReferences, reference => reference!.EndsWith(".Infrastructure", StringComparison.Ordinal));
            }
        }
    }

    [Fact]
    public void Foundation_contains_no_business_tables_or_migrations()
    {
        var source = string.Join('\n', Directory.EnumerateFiles(ServicesRoot, "*.cs", SearchOption.AllDirectories)
            .Select(File.ReadAllText));
        Assert.DoesNotContain("DbSet<", source, StringComparison.Ordinal);
        Assert.Empty(Directory.EnumerateDirectories(ServicesRoot, "Migrations", SearchOption.AllDirectories));
    }

    private static string? GetServiceName(string path)
    {
        var relative = Path.GetRelativePath(ServicesRoot, path);
        return relative.StartsWith("..", StringComparison.Ordinal) ? null : relative.Split(Path.DirectorySeparatorChar)[0];
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LedgerLens.slnx")))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException("Could not locate the LedgerLens repository root.");
    }
}
