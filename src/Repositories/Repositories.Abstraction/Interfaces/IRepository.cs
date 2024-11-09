using Repositories.Abstraction;

public interface IRepository
{
    string GetPath(RepositoryLocation location, string? sessionName = null);

    /// <summary>
    /// Return list of elements from repository.
    /// </summary>
    /// <param name="location">Location of files.</param>
    /// <param name="sessionName">Session name. Required if location is equal to session</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<RepositoryElementInfo> GetList(RepositoryLocation location, string? sessionName,
        CancellationToken cancellationToken = default);

    void SaveList(RepositoryLocation location, string? locationId, string? sessionName, string content, CancellationToken cancellationToken);
}