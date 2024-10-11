using Microsoft.Extensions.FileProviders;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Unicode;

public static class FileProviderFactory
{
    public static IFileProvider CreateFileProviderWithRootStructure()
    {
        // Create a mock IFileProvider
        var fileProvider = Substitute.For<IFileProvider>();

        // Create mock files and directories
        var rootDirectory = CreateMockDirectory("");
        var settingsDirectory = CreateMockDirectory("Settings");
        var commandsDirectory = CreateMockDirectory("Commands");
        var libraryDirectory = CreateMockDirectory("Library");
        var argumentsDirectory = CreateMockDirectory("Library/Arguments");
        var operationsDirectory = CreateMockDirectory("Library/Operations");

        var settingsFile = CreateMockFile("Settings/settings.json", "{\"name\":\"value\"}");
        var loggingJsonFile = CreateMockFile("Library/Arguments/logging.json");
        var loggingXmlFile = CreateMockFile("Library/Arguments/logging.xml");
        var testJsonFile = CreateMockFile("Library/Operations/test.json");

        // Build directory structure
        var rootDirectoryContents = new List<IFileInfo> { settingsDirectory, commandsDirectory, libraryDirectory };
        var libraryDirectoryContents = new List<IFileInfo> { argumentsDirectory, operationsDirectory };
        var argumentsDirectoryContents = new List<IFileInfo> { loggingJsonFile, loggingXmlFile };
        var operationsDirectoryContents = new List<IFileInfo> { testJsonFile };
        var settingsDirectoryContents = new List<IFileInfo> { settingsFile };

        // Setup directory contents to return appropriate files
        SetupDirectoryContents(fileProvider, "", rootDirectoryContents);
        SetupDirectoryContents(fileProvider, "Settings", settingsDirectoryContents);
        SetupDirectoryContents(fileProvider, "Commands", new List<IFileInfo>());
        SetupDirectoryContents(fileProvider, "Library", libraryDirectoryContents);
        SetupDirectoryContents(fileProvider, "Library/Arguments", argumentsDirectoryContents);
        SetupDirectoryContents(fileProvider, "Library/Operations", operationsDirectoryContents);

        // Setup file lookups
        SetupFile(fileProvider, "Settings/settings.json", settingsFile);
        SetupFile(fileProvider, "Library/Arguments/logging.json", loggingJsonFile);
        SetupFile(fileProvider, "Library/Arguments/logging.xml", loggingXmlFile);
        SetupFile(fileProvider, "Library/Operations/test.json", testJsonFile);
        
        return fileProvider;
    }

   
    // Creates a mock IFileInfo representing a file
    private static IFileInfo CreateMockFile(string fullPath, string? content = null)
    {
        var fileInfo = Substitute.For<IFileInfo>();
        fileInfo.Name.Returns(Path.GetFileName(fullPath));    // Set the file name
        fileInfo.IsDirectory.Returns(false);                 // Indicate it's a file
        fileInfo.Exists.Returns(true);                       // File exists
        fileInfo.Length.Returns(1024);                       // Arbitrary length for files
        fileInfo.LastModified.Returns(DateTimeOffset.Now);   // Set last modified date
        fileInfo.PhysicalPath.Returns(fullPath);             // Use the full path as PhysicalPath
        fileInfo.CreateReadStream().Returns(callInfo => GetFileContent(content));
        return fileInfo;
    }

    private static MemoryStream GetFileContent(string? content) =>
        content != null
            ? new MemoryStream(Encoding.UTF8.GetBytes(content))
            : throw new Exception("Missing content");

    // Creates a mock IFileInfo representing a directory
    private static IFileInfo CreateMockDirectory(string fullPath)
    {
        var directoryInfo = Substitute.For<IFileInfo>();
        directoryInfo.Name.Returns(Path.GetFileName(fullPath));   // Set the directory name
        directoryInfo.IsDirectory.Returns(true);                 // Indicate it's a directory
        directoryInfo.Exists.Returns(true);                      // Directory exists
        directoryInfo.Length.Returns(-1);                        // Length is irrelevant for directories
        directoryInfo.LastModified.Returns(DateTimeOffset.Now);  // Set last modified date
        directoryInfo.PhysicalPath.Returns(fullPath);            // Use the full path as PhysicalPath
        return directoryInfo;
    }

    private static void SetupDirectoryContents(IFileProvider fileProvider, string path, List<IFileInfo> contents)
    {
        var directoryContents = Substitute.For<IDirectoryContents>();
        directoryContents.Exists.Returns(true);
        using var enumerator = directoryContents.GetEnumerator();
        enumerator.Returns(contents.GetEnumerator());
        fileProvider.GetDirectoryContents(path).Returns(directoryContents);
    }

    private static void SetupFile(IFileProvider fileProvider, string path, IFileInfo fileInfo)
    {
        fileProvider.GetFileInfo(path).Returns(fileInfo);
    }
}
