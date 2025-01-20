using System.Runtime.CompilerServices;

/// <inheritdoc />

public class FileRepository(IPhysicalFileProvider physicalFileProvider, IOutputService outputService, IFlowService flowService) : IRepository
{
    /// <inheritdoc />
    public IAsyncEnumerable<RepositoryElementInformation> GetList(RepositoryLocation repositoryLocation, string? sessionName, CancellationToken cancellationToken)
    {
        try
        {
            var path = GetPath(repositoryLocation, sessionName);
            physicalFileProvider.CreateDirectoryIfItDoesNotExist(path);
            outputService.Trace($"Listing files in {repositoryLocation} repository at {path}");
            return GetFilesAsync(repositoryLocation, sessionName, cancellationToken);
        }
        catch (Exception ex)
        {
            flowService.NonFatalException(ex);
            outputService.Warning($"Failed to list files for location {repositoryLocation}");
            return AsyncEnumerable.Empty<RepositoryElementInformation>();
        }
    }

    /// <inheritdoc />
    public async Task SaveRepositoryElement(string repositoryElementId, string content, CancellationToken cancellationToken)
    {
        try
        {
            outputService.Trace($"Saving repository element: {repositoryElementId}");
            if (!Path.IsPathRooted(repositoryElementId))
                repositoryElementId = Path.Combine(GetPath(RepositoryLocation.Application), repositoryElementId);
            var directoryPath = Path.GetDirectoryName(repositoryElementId);

            if (!string.IsNullOrWhiteSpace(directoryPath))
                physicalFileProvider.CreateDirectoryIfItDoesNotExist(directoryPath);

            await physicalFileProvider.WriteFileAsync(repositoryElementId, content, cancellationToken);
            outputService.Debug($"Repository element saved successfully: {repositoryElementId}");
        }
        catch (Exception ex)
        {
            flowService.NonFatalException(ex);
            outputService.Warning($"Failed to save repository element {repositoryElementId}");
        }
    }

    /// <summary>
    /// Asynchronously retrieves serialized repository elements based on the specified location and session name.
    /// </summary>
    /// <param name="location">The repository location type.</param>
    /// <param name="sessionName">The optional session name for session-based repositories.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of serialized repository elements.</returns>
    private async IAsyncEnumerable<RepositoryElementInformation> GetFilesAsync(RepositoryLocation location, string? sessionName, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var rootPath = GetPath(location, sessionName);
        outputService.Debug($"Starting file enumeration in: {rootPath}");

        await foreach (var file in EnumerateFilesIterativelyAsync(rootPath, cancellationToken))
            yield return file;
    }

    /// <summary>
    /// Iteratively enumerates files within a directory without recursion and yields them as serialized repository elements.
    /// </summary>
    /// <param name="rootPath">The root directory to start the enumeration.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of serialized repository elements.</returns>
    private async IAsyncEnumerable<RepositoryElementInformation> EnumerateFilesIterativelyAsync(string rootPath, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var file in physicalFileProvider.GetFiles(rootPath))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                outputService.Debug("File enumeration canceled.");
                yield break;
            }

            var result = await TryGetRepositoryElementInfoAsync(rootPath, file, cancellationToken);
            if (result != null)
                yield return result;
        }

        await Task.Yield();
    }

    /// <summary>
    /// Attempts to retrieve repository element information from a file.
    /// </summary>
    /// <param name="rootPath">The root directory path for the repository.</param>
    /// <param name="file">The file path of the repository element.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="RepositoryElementInformation"/> object if successful; otherwise, <c>null</c>.</returns>
    internal Task<RepositoryElementInformation?> TryGetRepositoryElementInfoAsync(string rootPath, string file, CancellationToken cancellationToken)
    {
        try
        {
            var fileName = physicalFileProvider.GetFileName(file);
            if (fileName.StartsWith('.'))
            {
                outputService.Debug($"Skipping hidden/system file: {fileName}");
                return Task.FromResult<RepositoryElementInformation?>(null);
            }

            var format = Path.GetExtension(file).TrimStart('.');
            var friendlyName = file.Length > rootPath.Length
                ? file[(rootPath.Length + 1)..]
                : file;

            outputService.Trace($"File processed: {fileName}");
            return Task.FromResult<RepositoryElementInformation?>(new RepositoryElementInformation
            {
                Id = file, Format = format, FriendlyName = friendlyName,
                ContentAsString = new Lazy<string?>(() => physicalFileProvider.GetFileContentAsString(file)),
                ContentAsBinary = new Lazy<byte[]?>(() => physicalFileProvider.GetFileContentAsBinary(file))
            });
        }
        catch (Exception ex)
        {
            flowService.NonFatalException(ex);
            outputService.Warning($"Failed to process file {file}: {ex.Message}");
            return Task.FromResult<RepositoryElementInformation?>(null);
        }
    }

    /// <summary>
    /// Resolves the file path based on the specified repository location and session name.
    /// </summary>
    /// <param name="repositoryLocation">The repository location type.</param>
    /// <param name="sessionName">The optional session name for session-based repositories.</param>
    /// <returns>The resolved file path for the specified repository location and session name.</returns>
    /// <exception cref="ArgumentException">Thrown when the session name is not provided for session-based repositories or when an unsupported location is specified.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to access the built-in repository.</exception>
    public string GetPath(RepositoryLocation repositoryLocation, string? sessionName = null) =>
        repositoryLocation switch
        {
            RepositoryLocation.Application => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommandWeaver", "Global"),
            RepositoryLocation.Session when !string.IsNullOrWhiteSpace(sessionName) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommandWeaver", "Sessions", sessionName),
            RepositoryLocation.Session => throw new ArgumentException("SessionName not provided"),
            RepositoryLocation.BuiltIn => throw new InvalidOperationException("Built-in repository is not supported"),
            _ => throw new ArgumentException("Unsupported location")
        };
}
