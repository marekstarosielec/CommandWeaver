/// <summary>
/// Defines a service for managing repository operations, including retrieval, saving, and locating repository elements.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Retrieves a list of elements from the repository based on the specified location.
    /// </summary>
    /// <param name="repositoryLocation">The location of the repository files. This can be a specific location type or value.</param>
    /// <param name="sessionName">The session name to use if the <paramref name="repositoryLocation"/> is set to session-based retrieval.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete, allowing the operation to be canceled.</param>
    /// <returns>
    /// An asynchronous stream of <see cref="RepositoryElementSerialized"/> elements representing the items in the repository.
    /// </returns>
    IAsyncEnumerable<RepositoryElementSerialized> GetList(RepositoryLocation repositoryLocation, string? sessionName, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the physical path of the repository based on its location.
    /// </summary>
    /// <param name="repositoryLocation">The location of the repository to retrieve.</param>
    /// <param name="sessionName">
    /// An optional session name, used when retrieving the path for session-based repositories.
    /// </param>
    /// <returns>
    /// The physical file path or directory path where the repository is stored.
    /// </returns>
    /// <remarks>
    /// This method is used to determine the storage location of repository data, typically on the file system.
    /// </remarks>
    string GetPath(RepositoryLocation repositoryLocation, string? sessionName = null);

    /// <summary>
    /// Saves the content of a repository element back to the repository.
    /// </summary>
    /// <param name="repositoryElementId">The unique identifier of the repository element to save.</param>
    /// <param name="content">The content to be saved to the repository.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete, allowing the operation to be canceled.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous save operation.</returns>
    /// <remarks>
    /// This method persists changes to a repository element by saving its content back to its storage location, 
    /// typically a file on the file system.
    /// </remarks>
    Task SaveRepositoryElement(string repositoryElementId, string content, CancellationToken cancellationToken);
}
