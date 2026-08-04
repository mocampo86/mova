using System.Xml.Linq;

namespace Mova.ArchitectureTests;

public class DependencyDirectionTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    [Fact]
    public void Domain_has_no_project_references()
    {
        var references = GetProjectReferences("src/Mova.Domain/Mova.Domain.csproj");

        Assert.Empty(references);
    }

    [Fact]
    public void Contracts_has_no_project_references()
    {
        var references = GetProjectReferences("src/Mova.Contracts/Mova.Contracts.csproj");

        Assert.Empty(references);
    }

    [Fact]
    public void Application_references_Domain_and_Contracts_only()
    {
        var references = GetProjectReferences("src/Mova.Application/Mova.Application.csproj");

        Assert.Equal(["Mova.Contracts", "Mova.Domain"], references);
    }

    [Fact]
    public void Infrastructure_references_Domain_Application_and_Contracts()
    {
        var references = GetProjectReferences("src/Mova.Infrastructure/Mova.Infrastructure.csproj");

        Assert.Equal(["Mova.Application", "Mova.Contracts", "Mova.Domain"], references);
    }

    [Fact]
    public void Api_references_Application_Infrastructure_and_Contracts()
    {
        var references = GetProjectReferences("src/Mova.Api/Mova.Api.csproj");

        Assert.Equal(["Mova.Application", "Mova.Contracts", "Mova.Infrastructure"], references);
    }

    private static IEnumerable<string> GetProjectReferences(string relativeProjectPath)
    {
        var path = Path.Combine(RepoRoot, relativeProjectPath);
        var document = XDocument.Load(path);

        return document
            .Descendants("ProjectReference")
            .Select(e => e.Attribute("Include")?.Value ?? string.Empty)
            .Select(ExtractProjectName)
            .Where(n => !string.IsNullOrEmpty(n))
            .OrderBy(n => n)
            .ToList();
    }

    private static string ExtractProjectName(string includeValue)
    {
        var fileName = Path.GetFileName(includeValue);
        return fileName.Replace(".csproj", string.Empty, StringComparison.Ordinal);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (directory.GetFiles("Mova.slnx").Length > 0 ||
                directory.GetDirectories(".git").Length > 0)
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
