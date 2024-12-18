public interface IPhysicalFileProvider
{
    /// <summary>
    /// Creates a directory if it does not already exist at the specified path.
    /// </summary>
    /// <param name="directoryPath">The path where the directory should be created if missing.</param>
    void CreateDirectoryIfItDoesNotExist(string directoryPath);

    /// <summary>
    /// Retrieves all file paths within a directory and its subdirectories.
    /// </summary>
    /// <param name="directoryPath">The path of the directory to search.</param>
    /// <returns>A collection of all file paths in the specified directory and subdirectories.</returns>
    IEnumerable<string> GetFiles(string directoryPath);

    /// <summary>
    /// Gets the file name and extension from a file path.
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <returns>The file name and extension.</returns>
    string GetFileName(string filePath);

    /// <summary>
    /// Asynchronously reads the content of a file as a string.
    /// </summary>
    /// <param name="filePath">The full path of the file to read.</param>
    /// <returns>A task representing the asynchronous operation, containing the file content as a string.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the file cannot be accessed.</exception>
    string GetFileContentAsString(string filePath);

    /// <summary>
    /// Asynchronously reads the content of a file as a binary.
    /// </summary>
    /// <param name="filePath">The full path of the file to read.</param>
    /// <returns>A task representing the asynchronous operation, containing the file content as a string.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the file cannot be accessed.</exception>
    byte[] GetFileContentAsBinary(string filePath);
    
    /// <summary>
    /// Writes the specified content to a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="content">The content to write to the file.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken);
}

/// <inheritdoc />
public class PhysicalFileProvider(IFlowService flowService, IOutputService outputService) : IPhysicalFileProvider
{
    /// <inheritdoc />
    public void CreateDirectoryIfItDoesNotExist(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be null or whitespace.", nameof(directoryPath));

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
    }
    
    /// <inheritdoc />
    public IEnumerable<string> GetFiles(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be null or whitespace.", nameof(directoryPath));
        
        try
        {
            return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
        }
        catch (UnauthorizedAccessException ex)
        {
            flowService.NonFatalException(ex);
            outputService.Warning($"Failed to list files in {directoryPath}");
            return [];
        }
        catch (DirectoryNotFoundException ex)
        {
            flowService.NonFatalException(ex);
            outputService.Warning($"Failed to list files in {directoryPath}");
            return [];
        }
    }
    /// <inheritdoc />
    public string GetFileName(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        return Path.GetFileName(filePath);
    }

    /// <inheritdoc />
    public string GetFileContentAsString(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        return File.ReadAllText(filePath);
    }
    
    /// <inheritdoc />
    public byte[] GetFileContentAsBinary(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        
        return File.ReadAllBytes(filePath);
    }

    /// <inheritdoc />
    public async Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        try
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath != null)
                CreateDirectoryIfItDoesNotExist(directoryPath);

            await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(content.AsMemory(), cancellationToken);
        }
        catch (Exception ex)
        {
            flowService.NonFatalException(ex);
            outputService.Warning($"Failed to write to file: {filePath}");
        }
    }
}
