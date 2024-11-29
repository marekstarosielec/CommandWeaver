using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

public class FileRepositoryTests
{
    private readonly IPhysicalFileProvider _fileProvider;
    private readonly IOutputService _outputService;
    private readonly IFlowService _flowService;
    private readonly FileRepository _fileRepository;

    public FileRepositoryTests()
    {
        _fileProvider = Substitute.For<IPhysicalFileProvider>();
        _outputService = Substitute.For<IOutputService>();
        _flowService = Substitute.For<IFlowService>();
        _fileRepository = new FileRepository(_fileProvider, _outputService, _flowService);
    }

    [Fact]
    public async Task GetList_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        _fileProvider.When(x => x.CreateDirectoryIfItDoesNotExist(Arg.Any<string>()))
            .Do(x => throw new IOException("Test exception"));

        // Act
        var result = await _fileRepository
            .GetList(RepositoryLocation.Application, null, CancellationToken.None)
            .ToListAsync();

        // Assert
        Assert.Empty(result);
        _flowService.Received(1).NonFatalException(Arg.Any<IOException>());
        _outputService.Received(1).Warning(Arg.Is<string>(msg => msg.Contains("Failed to list files")));
    }

    [Fact]
    public async Task GetList_ShouldEnumerateFiles_WhenDirectoryExists()
    {
        // Arrange
        var testPath = _fileRepository.GetPath(RepositoryLocation.Application);
        var testFile = Path.Combine(testPath, "test-file.json");
        var testContent = "{ \"key\": \"value\" }";

        _fileProvider.GetFiles(testPath).Returns(new[] { testFile });
        _fileProvider.GetFileContent(testFile, Arg.Any<CancellationToken>()).Returns(testContent);

        // Act
        var result = await _fileRepository
            .GetList(RepositoryLocation.Application, null, CancellationToken.None)
            .ToListAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(testFile, result.First().Id);
        Assert.Equal("json", result.First().Format);
        Assert.Equal(testContent, result.First().Content);
        _outputService.Received(1).Trace(Arg.Is<string>(msg => msg.Contains("Listing files")));
    }

    [Fact]
    public async Task SaveRepositoryElement_ShouldCreateDirectoryAndSaveFile()
    {
        // Arrange
        var repositoryElementId = "testDir/testFile.txt";
        var content = "Sample content";

        var physicalFileProvider = Substitute.For<IPhysicalFileProvider>();
        var outputService = Substitute.For<IOutputService>();
        var flowService = Substitute.For<IFlowService>();

        var fileRepository = new FileRepository(physicalFileProvider, outputService, flowService);

        // Act
        await fileRepository.SaveRepositoryElement(repositoryElementId, content, CancellationToken.None);

        // Assert
        await physicalFileProvider.Received(1)
            .WriteFileAsync(repositoryElementId, content, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveRepositoryElement_ShouldHandleException()
    {
        // Arrange
        var testFile = "test-directory/test-file.json";
        var testContent = "{ \"key\": \"value\" }";

        _fileProvider.When(x => x.CreateDirectoryIfItDoesNotExist(Arg.Any<string>()))
            .Do(x => throw new IOException("Test exception"));

        // Act
        await _fileRepository.SaveRepositoryElement(testFile, testContent, CancellationToken.None);

        // Assert
        _flowService.Received(1).NonFatalException(Arg.Any<IOException>());
        _outputService.Received(1).Warning(Arg.Is<string>(msg => msg.Contains("Failed to save repository element")));
    }

    [Fact]
    public async Task GetPath_ShouldReturnValidPath_ForApplicationLocation()
    {
        // Act
        var path = _fileRepository.GetPath(RepositoryLocation.Application);

        // Assert
        Assert.Contains("CommandWeaver", path);
        Assert.Contains("Global", path);
    }

    [Fact]
    public void GetPath_ShouldThrowException_ForBuiltInRepositoryLocation()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _fileRepository.GetPath(RepositoryLocation.BuiltIn));
        Assert.Equal("Built-in repository is not supported", exception.Message);
    }

    [Fact]
    public async Task TryGetRepositoryElementInfo_ShouldSkipHiddenFiles()
    {
        // Arrange
        var rootPath = "root";
        var hiddenFile = ".hidden-file.json";
        _fileProvider.GetFileName(hiddenFile).Returns(hiddenFile);

        // Act
        var result = await _fileRepository.TryGetRepositoryElementInfoAsync(rootPath, hiddenFile, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _outputService.Received(1).Debug(Arg.Is<string>(msg => msg.Contains("Skipping hidden/system file")));
    }

    [Fact]
    public async Task TryGetRepositoryElementInfo_ShouldProcessValidFile()
    {
        // Arrange
        var rootPath = "root";
        var validFile = "file.json";
        var testContent = "{ \"key\": \"value\" }";

        _fileProvider.GetFileName(validFile).Returns(validFile);
        _fileProvider.GetFileContent(validFile, Arg.Any<CancellationToken>()).Returns(testContent);

        // Act
        var result = await _fileRepository.TryGetRepositoryElementInfoAsync(rootPath, validFile, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(validFile, result!.Id);
        Assert.Equal("json", result.Format);
        Assert.Equal(testContent, result.Content);
        _outputService.Received(1).Trace(Arg.Is<string>(msg => msg.Contains("File processed")));
    }
}
