using System.Reflection;
using System.Runtime.CompilerServices;

public class FileRepository(IPhysicalFileProvider physicalFileProvider, IOutput output) : IRepository
{
    /// <inheritdoc />
    public IAsyncEnumerable<RepositoryElement> GetList(RepositoryLocation repositoryLocation, string? sessionName, CancellationToken cancellationToken)
    {
        try
        {
            var path = GetPath(repositoryLocation, sessionName);
            physicalFileProvider.CreateDirectoryIfItDoesNotExist(path);
            return GetFilesAsync(repositoryLocation, sessionName, cancellationToken);
        }
        catch
        {
            output.Warning($"Failed to list location {repositoryLocation.ToString()}");
            return AsyncEnumerable.Empty<RepositoryElement>();
        }
    }

    public void SaveList(RepositoryLocation location, string? repositoryElementId, string? sessionName, string content, CancellationToken cancellationToken)
    {
        var path = GetPath(location, sessionName);
        var directoryPath = Path.GetDirectoryName(Path.Combine(path, repositoryElementId.TrimStart('\\')));

        if (directoryPath != null)
            Directory.CreateDirectory(directoryPath);

        using var stream = new FileStream(Path.Combine(path, repositoryElementId.TrimStart('\\')), FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        writer.Write(content);
    }
  
    internal async IAsyncEnumerable<RepositoryElement> GetFilesAsync(RepositoryLocation location, string? sessionName, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var rootPath = GetPath(location, sessionName);
        await foreach (var file in EnumerateFilesIterativelyAsync(rootPath, cancellationToken))
           yield return file;
    }

    /// <summary>
    /// Iteratively enumerates files and directories using a stack to avoid recursion issues.
    /// Errors encountered are logged to the result's error collection.
    /// </summary>
    private async IAsyncEnumerable<RepositoryElement> EnumerateFilesIterativelyAsync(string rootPath, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var file in physicalFileProvider.GetFiles(rootPath))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            var result = await TryGetRepositoryElementInfoAsync(rootPath, file, cancellationToken);
            if (result != null)
                yield return result;
        }

        await Task.Yield(); // Allow other asynchronous operations to run
    }

    /// <summary>
    /// Read information and content of file in try/catch.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="basePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<RepositoryElement?> TryGetRepositoryElementInfoAsync(string rootPath, string file, CancellationToken cancellationToken)
    {
        try
        {
            var fileName = physicalFileProvider.GetFileName(file);

            //Skip system/hidden files.
            if (fileName.StartsWith('.'))
                return null;

            var format = file.LastIndexOf('.') is var pos && pos >= 0 ? file[(pos + 1)..] : string.Empty;
            var friendlyName = file.Length > rootPath.Length
                ? file[(rootPath.Length + 1)..]
                : file;

            var content = await physicalFileProvider.GetFileContent(file, cancellationToken);
            return new RepositoryElement { Id = file, Format = format, FriendlyName = friendlyName, Content = content };
        }
        catch (Exception ex)
        {
            // Log the exception and return null to skip this file
            output.Warning($"Failed to process file {file}");
            return null;
        }
    }

    /// <summary>
    /// Gets the path based on the specified repository location and session name.
    /// </summary>
    private string GetPath(RepositoryLocation location, string? sessionName = null)
    {
        return location switch
        {
            RepositoryLocation.Application => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommandWeaver"),
            RepositoryLocation.Session when !string.IsNullOrWhiteSpace(sessionName) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommandWeaver", "Sessions", sessionName),
            RepositoryLocation.Session => throw new ArgumentException("SessionName not provided"),
            RepositoryLocation.BuiltIn => throw new NotImplementedException(),
            _ => throw new ArgumentException("Unsupported location")
        };
    }
}