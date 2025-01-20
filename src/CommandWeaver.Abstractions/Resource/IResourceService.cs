/// <summary>
/// Test for resources - files accessible from commands.
/// </summary>
public interface IResourceService
{
    /// <summary>
    /// Add new resource to list.
    /// </summary>
    /// <param name="repositoryElementInformation"></param>
    void Add(RepositoryElementInformation repositoryElementInformation);
}