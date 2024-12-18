using System.Reflection;
using System.Text;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using NSubstitute;

public class EmbeddedRepositoryTests
{
    private readonly IOutputService _mockOutputService;
    private readonly IFlowService _mockFlowService;
    private readonly Assembly _mockAssembly;

    public EmbeddedRepositoryTests()
    {
        _mockOutputService = Substitute.For<IOutputService>();
        _mockFlowService = Substitute.For<IFlowService>();
        _mockAssembly = Substitute.For<Assembly>();
    }

    [Fact]
    public async Task GetList_ShouldReturnSerializedRepositoryElements_ForValidJsonResources()
    {
        // Arrange
        var resourceNames = new[] { "TestResources.Resource1.json", "TestResources.Resource2.json" };
        _mockAssembly.GetManifestResourceNames().Returns(resourceNames);

        // Mock GetName to return a valid AssemblyName
        var mockAssemblyName = new AssemblyName("TestAssembly");
        _mockAssembly.GetName().Returns(mockAssemblyName);

        _mockAssembly.GetManifestResourceStream(Arg.Is<string>(s => s.EndsWith(".json")))
            .Returns(callInfo =>
            {
                var resourceName = callInfo.Arg<string>();
                return new MemoryStream(Encoding.UTF8.GetBytes($"Content of {resourceName}"));
            });

        var repository = new EmbeddedRepository(_mockAssembly, "TestResources.", _mockOutputService, _mockFlowService);

        // Act
        var results = new List<RepositoryElementInformation>();
        await foreach (var element in repository.GetList(CancellationToken.None))
        {
            results.Add(element);
        }

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.Equal("json", e.Format));
        Assert.All(results, e => Assert.Contains("Content of", e.ContentAsString?.Value));
        Assert.All(results, e => Assert.StartsWith("TestResources\\", e.Id));
    }
    
    [Fact]
    public async Task GetList_ShouldFilterNonPrefixedResources()
    {
        // Arrange
        var resourceNames = new[] { "OtherResources.Resource1.json", "TestResources.Resource2.json" };
        _mockAssembly.GetManifestResourceNames().Returns(resourceNames);

        // Mock GetName to return a valid AssemblyName
        var mockAssemblyName = new AssemblyName("TestAssembly");
        _mockAssembly.GetName().Returns(mockAssemblyName);

        _mockAssembly.GetManifestResourceStream("TestResources.Resource2.json")
            .Returns(new MemoryStream(Encoding.UTF8.GetBytes("Valid JSON content")));

        var repository = new EmbeddedRepository(_mockAssembly, "TestResources.", _mockOutputService, _mockFlowService);

        // Act
        var results = new List<RepositoryElementInformation>();
        await foreach (var element in repository.GetList(CancellationToken.None))
        {
            results.Add(element);
        }

        // Assert
        Assert.Single(results); // Only prefixed JSON resource is processed
        Assert.Equal("TestResources\\Resource2.json", results[0].Id);
    }
    
    [Fact]
    public void AllJsonFilesInProject_ShouldBeEmbeddedResources()
    {
        // Arrange
        if (!MSBuildLocator.IsRegistered)
            MSBuildLocator.RegisterDefaults();

        var testedAssemblyName = "CommandWeaver.Repositories.Embedded";
        var assembly = Assembly.Load(testedAssemblyName);
        var projectDirectory = GetProjectDirectory(testedAssemblyName);

        // Ensure project directory exists
        Assert.True(Directory.Exists(projectDirectory), $"Project directory not found: {projectDirectory}");

        // Find all .json files in the project directory, excluding obj and bin folders
        var jsonFiles = Directory.GetFiles(Path.Combine(projectDirectory, "BuiltIn"), "*.*", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                           !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Select(Path.GetFullPath)
            .ToList();

        // Get embedded resource names from the assembly
        var embeddedResourceNames = assembly.GetManifestResourceNames()
            .ToList();

        // Act
        var missingEmbeddedResources = jsonFiles.Where(file =>
        {
            // Convert file path to the format used in the assembly's embedded resource names
            var relativePath = testedAssemblyName + "." + file.Substring(projectDirectory.Length + 1)
                .Replace(Path.DirectorySeparatorChar, '.')
                .Replace("-", "_"); // Replace dashes with underscores for matching

            // Match resource names flexibly (handle both - and _)
            return !embeddedResourceNames.Any(resource =>
                string.Equals(resource.Replace("-", "_"), relativePath, StringComparison.OrdinalIgnoreCase));
        }).ToList();

        if (missingEmbeddedResources.Any())
        {
            var missingFiles = string.Join("\n", missingEmbeddedResources);
            Assert.Fail($"The following .json files are not marked as EmbeddedResource:\n{missingFiles}");
        }
    }


    private static string? GetProjectDirectory(string projectName)
    {
        // Start searching from the current directory
        var currentDirectory = Directory.GetCurrentDirectory();

        // Look for a solution file in the current directory or its parents
        var solutionPath = FindSolutionFile(currentDirectory);
        if (solutionPath == null)
            throw new FileNotFoundException("No solution file found in the current directory or its parent directories.");
        
        // Load the solution file
        var solution = SolutionFile.Parse(solutionPath);

        // Search for the project in the solution
        var project = solution.ProjectsInOrder.FirstOrDefault(p =>
            Path.GetFileNameWithoutExtension(p.AbsolutePath).Equals(projectName, StringComparison.OrdinalIgnoreCase));

        // Return the project directory, or null if not found
        return project != null ? Path.GetDirectoryName(project.AbsolutePath) : null;
    }
    
    private static string? FindSolutionFile(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory != null)
        {
            // Check for solution files in the current directory
            var solutionFiles = directory.GetFiles("*.sln", SearchOption.TopDirectoryOnly);
            if (solutionFiles.Any())
                return solutionFiles.First().FullName; // Return the first solution file found
        
            // Move to the parent directory
            directory = directory.Parent;
        }

        return null; // No solution file found
    }
}
