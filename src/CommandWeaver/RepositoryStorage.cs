
public record Repository(RepositoryLocation repositoryLocation, string id, RepositoryElementContent? content);

/// <summary>
/// Interface for storing original contents of repositories. 
/// </summary>
public interface IRepositoryStorage
{
    void Add(Repository repository);
}

public class RepositoryStorage : IRepositoryStorage
{
    private readonly List<Repository> _repositories = [];

    public void Add(Repository repository) => _repositories.Add(repository);
}