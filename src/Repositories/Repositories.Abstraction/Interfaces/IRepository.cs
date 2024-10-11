using Models;

namespace Repositories.Abstraction.Interfaces;

public interface IRepository
{
    /// <summary>
    /// Return list of elements from repository.
    /// </summary>
    /// <param name="location">Location of files.</param>
    /// <param name="sessionName">Session name. Required if location is equal to session</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<RepositoryElementInfo> GetList(RepositoryLocation location, string? sessionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns content of element.
    /// </summary>
    /// <param name="location">Location of files.</param>
    /// <param name="sessionName">Session name. Required if location is equal to session</param>
    /// <param name="id">Id of element to read content.</param>
    /// <returns></returns>
    Task<RepositoryElementContent> GetContent(RepositoryLocation location, string? sessionName, string id);
}