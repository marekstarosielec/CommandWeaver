public interface IRepository
{
    /// <summary>
    /// Retrieves a list of elements from the embedded repository based on the specified location.
    /// </summary>
    /// <param name="repositoryLocation">The location of the repository files. This can be a specific location type or value.</param>
    /// <param name="sessionName">The session name to use if the <paramref name="repositoryLocation"/> is set to session-based retrieval.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete, allowing the operation to be canceled.</param>
    /// <returns>An asynchronous stream of <see cref="RepositoryElement"/> elements representing the items in the repository.</returns>
    IAsyncEnumerable<RepositoryElement> GetList(RepositoryLocation repositoryLocation, string? sessionName, CancellationToken cancellationToken);

    string GetPath(RepositoryLocation repositoryLocation, string? sessionName = null);

    void SaveList(RepositoryLocation location, string? repositoryElementId, string? sessionName, string content, CancellationToken cancellationToken);
}