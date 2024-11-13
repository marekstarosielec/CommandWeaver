
using System.Collections.Immutable;

public record RepositoryElement(RepositoryLocation repositoryLocation, string id, RepositoryElementContent? content);

/// <summary>
/// Interface for storing original contents of repositories. 
/// </summary>
public interface IRepositoryStorage
{
    void Add(RepositoryElement repository);

    ImmutableList<RepositoryElement> Get();
}

public class RepositoryElementStorage : IRepositoryStorage
{
    private readonly List<RepositoryElement> _repositories = [];

    public void Add(RepositoryElement repository) => _repositories.Add(repository);

    public ImmutableList<RepositoryElement> Get() => _repositories.ToImmutableList();
}