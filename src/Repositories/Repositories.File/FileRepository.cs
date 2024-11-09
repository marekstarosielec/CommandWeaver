using System.Runtime.CompilerServices;
using Microsoft.Extensions.FileProviders;
using Repositories.Abstraction;

namespace Repositories.File;

public class FileRepository : IRepository
{
    protected internal string BuiltInFolder { get; internal set; } = Path.Combine(AppContext.BaseDirectory, "BuiltIn");
    protected internal string LocalFolder { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cli2");

    /// <inheritdoc />
    public IAsyncEnumerable<RepositoryElementInfo> GetList(RepositoryLocation location, string? sessionName, CancellationToken cancellationToken = default)
    {
        try
        {
            var path = GetPath(location, sessionName);
            //TODO: Hide it in some interfaces
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var fileProvider = new PhysicalFileProvider(path);
            return GetFilesAsync(location, sessionName, fileProvider, cancellationToken);
        }
        catch
        {
            //TODO: do something when path cannot be read? Output?
            return AsyncEnumerable.Empty<RepositoryElementInfo>();
        }
    }

    public void SaveList(RepositoryLocation location, string? locationId, string? sessionName, string content, CancellationToken cancellationToken)
    {
        var path = GetPath(location, sessionName);
        var fileProvider = new PhysicalFileProvider(path);
        var fileInfo = fileProvider.GetFileInfo(locationId);

        var directoryPath = Path.GetDirectoryName(Path.Combine(path, locationId.TrimStart('\\')));

        if (directoryPath != null)
            Directory.CreateDirectory(directoryPath);

        using (var stream = new FileStream(Path.Combine(path, locationId.TrimStart('\\')), FileMode.Create, FileAccess.Write))
        {
            using (var writer = new StreamWriter(stream))
            {
                // Move the writer to the end if appending; otherwise, overwrite content
                stream.Seek(0, SeekOrigin.End); // remove this line if you want to overwrite instead of append
                writer.Write(content);
            }
        }
    }
  
    internal async IAsyncEnumerable<RepositoryElementInfo> GetFilesAsync(RepositoryLocation location, string? sessionName, IFileProvider fileProvider,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var rootPath = GetPath(location, sessionName);
        
        await foreach (var file in EnumerateFilesIterativelyAsync(rootPath, fileProvider, cancellationToken))
           yield return file;
    }

    /// <summary>
    /// Iteratively enumerates files and directories using a stack to avoid recursion issues.
    /// Errors encountered are logged to the result's error collection.
    /// </summary>
    private async IAsyncEnumerable<RepositoryElementInfo> EnumerateFilesIterativelyAsync(string rootPath, IFileProvider fileProvider, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var directoriesToProcess = new Stack<string>();
        directoriesToProcess.Push(string.Empty); // Start from the root directory

        while (directoriesToProcess.Count > 0)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            var currentPath = directoriesToProcess.Pop();
         
            // Attempt to get the directory contents and log any errors encountered
            IDirectoryContents? directoryContents = null;
            Exception? exception = null;
            try
            {
                directoryContents = fileProvider.GetDirectoryContents(currentPath);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
                yield return new RepositoryElementInfo { Id = currentPath, Exception = exception };
            if (directoryContents == null)
                yield return new RepositoryElementInfo
                    { Id = currentPath, Exception = new Exception("Unknown exception") };
            if (!directoryContents!.Exists)
                yield return new RepositoryElementInfo
                    { Id = currentPath, Exception = new Exception("Path does not exist") };
            
            foreach (var fileInfo in directoryContents!)
            {
                if (cancellationToken.IsCancellationRequested || fileInfo.PhysicalPath == null)
                    yield break;
                var relativePath = GetRelativePath(rootPath, fileInfo.PhysicalPath);
                if (fileInfo.IsDirectory)
                {
                    directoriesToProcess.Push(relativePath);
                    continue;
                }
                yield return new RepositoryElementInfo { Id = relativePath, Format = GetFormat(fileInfo.Name), FriendlyName = GetFriendlyName(rootPath, fileInfo.PhysicalPath), Content = await GetContent2(rootPath, relativePath) };
            }
            await Task.Yield(); // Allow other asynchronous operations to run
        }
    }

    private async Task<string> GetContent2(string rootPath, string relativePath)
    {
        //try
        //{
        //TODO: Add ability to replace fileProvider for testing.
            var fileProvider = new PhysicalFileProvider(rootPath);
            var fileInfo = fileProvider.GetFileInfo(relativePath);
            await using var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(); //TODO: Pass cancellationToken
        //return new RepositoryElementContent
        //{
        //    Id = id,
        //    Content = await reader.ReadToEndAsync()
        //};
        //TODO: When reading error, it should put exception in RepositoryElementInfo.
        //}
        //catch (Exception e)
        //{
        //    return new RepositoryElementContent
        //    {
        //        Id = id,
        //        Exception = e
        //    };
        //}
    }

    private string GetRelativePath(string rootPath, string physicalPath) => physicalPath[rootPath.Length..];

    private string? GetFormat(string fileName) => Path.GetExtension(fileName).TrimStart('.');

    private string? GetFriendlyName(string rootPath, string physicalPath) => GetRelativePath(rootPath, physicalPath).TrimStart('/');

    /// <summary>
    /// Gets the path based on the specified repository location and session name.
    /// </summary>
    public string GetPath(RepositoryLocation location, string? sessionName = null) =>
        location switch
        {
            RepositoryLocation.BuiltIn => BuiltInFolder,
            RepositoryLocation.Application => LocalFolder,
            RepositoryLocation.Session when !string.IsNullOrWhiteSpace(sessionName) => Path.Combine(LocalFolder, "Sessions", sessionName),
            RepositoryLocation.Session => throw new ArgumentException("SessionName not provided"),
            _ => throw new ArgumentException("Unsupported location")
        };
}