using System.Collections.Immutable;

/// <summary>
/// Interface for storing and managing repository elements. 
/// This storage contains values retrieved from repositories and tracks changes made during command execution.
/// After command execution, the data is saved back to the repository.
/// This service should be injected as a singleton to ensure all services access the same dataset.
/// </summary>
public interface IRepositoryElementStorage
{
    /// <summary>
    /// Adds a repository element to the storage.
    /// </summary>
    /// <param name="repository">The repository element to add.</param>
    void Add(RepositoryElement repository);

    /// <summary>
    /// Retrieves all repository elements stored, including their original values and any changes.
    /// </summary>
    /// <returns>An immutable list of repository elements.</returns>
    ImmutableList<RepositoryElement> Get();
}

/// <inheritdoc />
public class RepositoryElementStorage : IRepositoryElementStorage
{
    private readonly List<RepositoryElement> _repositories = [];

    /// <inheritdoc />
    public void Add(RepositoryElement repository) => _repositories.Add(repository);

    /// <inheritdoc />
    public ImmutableList<RepositoryElement> Get() => _repositories.ToImmutableList();
}