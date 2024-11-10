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
    /// Gets the directory path from a full file path.
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <returns>The directory path.</returns>
    string GetDirectoryName(string filePath);

    /// <summary>
    /// Retrieves the file extension from a file name or path, without the leading dot.
    /// </summary>
    /// <param name="filePath">The full path or name of the file.</param>
    /// <returns>The file extension without the leading dot.</returns>
    string GetExtension(string filePath);

    /// <summary>
    /// Asynchronously reads the content of a file as a string.
    /// </summary>
    /// <param name="filePath">The full path of the file to read.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the file content as a string.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the file cannot be accessed.</exception>
    Task<string> GetFileContent(string filePath, CancellationToken cancellationToken);
}

/// <inheritdoc />
public class PhysicalFileProvider : IPhysicalFileProvider
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

        return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
    }

    /// <inheritdoc />
    public string GetFileName(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        return Path.GetFileName(filePath);
    }

    /// <inheritdoc />
    public string GetDirectoryName(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        return Path.GetDirectoryName(filePath) ?? string.Empty;
    }

    /// <inheritdoc />
    public string GetExtension(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        return Path.GetExtension(filePath).TrimStart('.');
    }

    /// <inheritdoc />
    public async Task<string> GetFileContent(string filePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        return await File.ReadAllTextAsync(filePath, cancellationToken);
    }
}
