using System.Runtime.CompilerServices;
using Microsoft.Extensions.FileProviders;
using Models;
using Repositories.Abstraction;
using Repositories.Abstraction.Interfaces;

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
            var path = GetDiscPath(location, sessionName);
            //TODO: Hide it in some interfaces
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var fileProvider = new PhysicalFileProvider(GetDiscPath(location, sessionName));
            return GetFilesAsync(location, sessionName, fileProvider, cancellationToken);
        }
        catch
        {
            //TODO: do something when path cannot be read? Output?
            return AsyncEnumerable.Empty<RepositoryElementInfo>();
        }
    }

    /// <inheritdoc />
    public Task<RepositoryElementContent> GetContent(RepositoryLocation location, string? sessionName, string id) =>
        GetContent(id, new PhysicalFileProvider(GetDiscPath(location, sessionName)));
    
    internal async IAsyncEnumerable<RepositoryElementInfo> GetFilesAsync(RepositoryLocation location, string? sessionName, IFileProvider fileProvider,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var rootPath = GetDiscPath(location, sessionName);
        
        await foreach (var file in EnumerateFilesIterativelyAsync(rootPath, fileProvider, cancellationToken))
           yield return file;
    }

    internal async Task<RepositoryElementContent> GetContent(string id, IFileProvider fileProvider)
    {
        try
        {
            var fileInfo = fileProvider.GetFileInfo(id);
            await using var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            return new RepositoryElementContent
            {
                Id = id,
                Content = await reader.ReadToEndAsync()
            };
        }
        catch (Exception e)
        {
            return new RepositoryElementContent
            {
                Id = id,
                Exception = e
            };
        }
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
                yield return new RepositoryElementInfo { Id = relativePath, Type = GetType(rootPath, fileInfo.PhysicalPath), Format = GetFormat(fileInfo.Name), FriendlyName = GetFriendlyName(rootPath, fileInfo.PhysicalPath)};
            }
            await Task.Yield(); // Allow other asynchronous operations to run
        }
    }

    private string GetRelativePath(string rootPath, string physicalPath) => physicalPath[rootPath.Length..];

    private string? GetType(string rootPath, string physicalPath) => GetRelativePath(rootPath, physicalPath).TrimStart('/').Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
    
    private string? GetFormat(string fileName) => Path.GetExtension(fileName).TrimStart('.');

    private string? GetFriendlyName(string rootPath, string physicalPath) => GetRelativePath(rootPath, physicalPath).TrimStart('/');

    /// <summary>
    /// Gets the path based on the specified repository location and session name.
    /// </summary>
    internal string GetDiscPath(RepositoryLocation location, string? sessionName) =>
        location switch
        {
            RepositoryLocation.BuiltIn => BuiltInFolder,
            RepositoryLocation.Local => LocalFolder,
            RepositoryLocation.Session when !string.IsNullOrWhiteSpace(sessionName) => Path.Combine(LocalFolder, "Sessions", sessionName),
            RepositoryLocation.Session => throw new ArgumentException("SessionName not provided"),
            _ => throw new ArgumentException("Unsupported location")
        };
}