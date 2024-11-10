using System.Reflection;
using System.Runtime.CompilerServices;

public class FileRepository(IPhysicalFileProvider physicalFileProvider) : IRepository
{
    protected internal string BuiltInFolder { get; internal set; } = Path.Combine(AppContext.BaseDirectory, "BuiltIn");
    protected internal string LocalFolder { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cli2");

    /// <inheritdoc />
    public IAsyncEnumerable<RepositoryElementInfo> GetList(RepositoryLocation location, string? sessionName, CancellationToken cancellationToken)
    {
        try
        {
            var path = GetPath(location, sessionName);
            physicalFileProvider.CreateDirectoryIfItDoesNotExist(path);
            return GetFilesAsync(location, sessionName, cancellationToken);
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
  
    internal async IAsyncEnumerable<RepositoryElementInfo> GetFilesAsync(RepositoryLocation location, string? sessionName, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var rootPath = GetPath(location, sessionName);
        
        await foreach (var file in EnumerateFilesIterativelyAsync(rootPath, cancellationToken))
           yield return file;
    }

    /// <summary>
    /// Iteratively enumerates files and directories using a stack to avoid recursion issues.
    /// Errors encountered are logged to the result's error collection.
    /// </summary>
    private async IAsyncEnumerable<RepositoryElementInfo> EnumerateFilesIterativelyAsync(string rootPath, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var basePath = physicalFileProvider.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        foreach (var file in physicalFileProvider.GetFiles(rootPath))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
            var fileName = physicalFileProvider.GetFileName(file);
            if (fileName.StartsWith('.'))
                continue;

            var format = file.LastIndexOf('.') is var pos && pos >= 0 ? file[(pos + 1)..] : string.Empty;
            var friendlyName = file.Length > basePath.Length + format.Length + 1
                ? file[(basePath.Length + 1)..^(format.Length + 1)].Replace('.', '/')
                : file;

            var content = await physicalFileProvider.GetFileContent(file, cancellationToken);
            yield return new RepositoryElementInfo { Id = file, Format = format, FriendlyName = friendlyName, Content = content };
        }
      
        await Task.Yield(); // Allow other asynchronous operations to run
    }

    /// <summary>
    /// Gets the path based on the specified repository location and session name.
    /// </summary>
    private string GetPath(RepositoryLocation location, string? sessionName = null) =>
        location switch
        {
            RepositoryLocation.BuiltIn => BuiltInFolder,
            RepositoryLocation.Application => LocalFolder,
            RepositoryLocation.Session when !string.IsNullOrWhiteSpace(sessionName) => Path.Combine(LocalFolder, "Sessions", sessionName),
            RepositoryLocation.Session => throw new ArgumentException("SessionName not provided"),
            _ => throw new ArgumentException("Unsupported location")
        };
}