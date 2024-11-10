public interface IRepository
{
    /// <summary>
    /// Retrieves a list of elements from the embedded repository based on the specified location.
    /// </summary>
    /// <param name="location">The location of the repository files. This can be a specific location type or value.</param>
    /// <param name="sessionName">The session name to use if the <paramref name="location"/> is set to session-based retrieval.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete, allowing the operation to be canceled.</param>
    /// <returns>An asynchronous stream of <see cref="RepositoryElementInfo"/> elements representing the items in the repository.</returns>
    IAsyncEnumerable<RepositoryElementInfo> GetList(RepositoryLocation location, string? sessionName, CancellationToken cancellationToken);

    void SaveList(RepositoryLocation location, string? locationId, string? sessionName, string content, CancellationToken cancellationToken);
}