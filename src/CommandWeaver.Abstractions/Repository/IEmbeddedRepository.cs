/// <summary>
/// Defines a repository interface for retrieving embedded elements.
/// </summary>
public interface IEmbeddedRepository
{
    /// <summary>
    /// Retrieves a list of elements from the embedded repository based on the specified location.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete, allowing the operation to be canceled.</param>
    /// <returns>An asynchronous stream of <see cref="RepositoryElementInformation"/> elements representing the items in the repository.</returns>
    IAsyncEnumerable<RepositoryElementInformation> GetList(CancellationToken cancellationToken);
}