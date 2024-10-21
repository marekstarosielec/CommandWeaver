using Models;
using Repositories.Abstraction;
namespace Repositories.File.Tests;

public class FileRepositoryTests
{
    [Fact]
    public void GetPath_DoesNotThrow_ForAnyRepositoryLocation()
    {
        var sut = new FileRepository();
        foreach (var repositoryLocation in Enum.GetValues<RepositoryLocation>())
        {
            var exception = Record.Exception(() => sut.GetPath(repositoryLocation, "sessionName"));
            Assert.True(exception == null, $"{repositoryLocation} is not properly handled in GetPath");
        }
    }

    [Fact]
    public async Task GetFilesAsync_ListsAllFiles()
    {
        var sut = new FileRepository
        {
            BuiltInFolder = "",
            LocalFolder = ""
        };
        
        var result = await sut.GetFilesAsync(RepositoryLocation.Local, null, FileProviderFactory.CreateFileProviderWithRootStructure()).ToListAsync();
        Assert.True(result.Count == 4, "Invalid number of elements returned");
        Assert.True(result[0].Id == "Library/Operations/test.json", "Invalid id returned");
        Assert.True(result[0].Type == "Library", "Invalid type returned");
        Assert.True(result[0].Format == "json", "Invalid format returned");
        Assert.True(result[0].FriendlyName == "Library/Operations/test.json", "Invalid friendly name returned");
        Assert.True(result[1].Id == "Library/Arguments/logging.json", "Invalid id returned");
        Assert.True(result[1].Type == "Library", "Invalid type returned");
        Assert.True(result[1].Format == "json", "Invalid format returned");
        Assert.True(result[1].FriendlyName == "Library/Arguments/logging.json", "Invalid friendly name returned");
        Assert.True(result[2].Id == "Library/Arguments/logging.xml", "Invalid id returned");
        Assert.True(result[2].Type == "Library", "Invalid type returned");
        Assert.True(result[2].Format == "xml", "Invalid format returned");
        Assert.True(result[2].FriendlyName == "Library/Arguments/logging.xml", "Invalid friendly name returned");
        Assert.True(result[3].Id == "Settings/settings.json", "Invalid id returned");
        Assert.True(result[3].Type == "Settings", "Invalid type returned");
        Assert.True(result[3].Format == "json", "Invalid format returned");
        Assert.True(result[3].FriendlyName == "Settings/settings.json", "Invalid friendly name returned");
    }

    [Fact]
    public async Task GetFilesAsync_ReturnsContents_WhenFileContentsAreValid()
    {
        var sut = new FileRepository
        {
            BuiltInFolder = "",
            LocalFolder = ""
        };
        var result = await sut.GetContent("Settings/settings.json", FileProviderFactory.CreateFileProviderWithRootStructure());
        Assert.NotNull(result?.Content);
        Assert.Null(result?.Exception);
    }
    
    [Fact]
    public async Task GetFilesAsync_ThrowsException_WhenFileContentsAreInvalid()
    {
        var sut = new FileRepository
        {
            BuiltInFolder = "",
            LocalFolder = ""
        };
        var result = await sut.GetContent("Library/Arguments/logging.json", FileProviderFactory.CreateFileProviderWithRootStructure());
        Assert.Null(result?.Content);
        Assert.NotNull(result?.Exception);
    }
}