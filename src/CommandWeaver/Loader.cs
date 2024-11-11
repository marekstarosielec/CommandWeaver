public interface ILoader
{
    Task Execute(CancellationToken cancellationToken);
}

public class Loader(
    IVariables variables, 
    IEmbeddedRepository 
    embeddedRepository, 
    IOutput output, 
    ICommands commands,
    ISerializerFactory serializerFactory) : ILoader
{
    public Task Execute(CancellationToken cancellationToken)
    {
        return LoadRepositories(cancellationToken);
    }

    /// <summary>
    /// Loads commands and variables from all defined repositories.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task LoadRepositories(CancellationToken cancellationToken)
    {
        variables.CurrentlyLoadRepository = "built-in";
        var builtInElements = embeddedRepository.GetList(cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.BuiltIn, null, builtInElements);

        variables.CurrentlyLoadRepository = null;
    }

    /// <summary>
    /// Loads commands and variables from repository.
    /// </summary>
    /// <param name="repositoryLocation"></param>
    /// <param name="sessionName"></param>
    /// <param name="repositoryElements"></param>
    /// <returns></returns>
    private async Task LoadRepositoryElements(RepositoryLocation repositoryLocation, string? sessionName, IAsyncEnumerable<RepositoryElement> repositoryElements)
    {
        await foreach (var repositoryElement in repositoryElements)
        {
            variables.CurrentlyLoadRepositoryElement = repositoryElement.FriendlyName;
            output.Debug($"Processing element {variables.CurrentlyLoadRepositoryElement}");

            if (string.IsNullOrWhiteSpace(repositoryElement.Content))
            {
                output.Warning($"Element {variables.CurrentlyLoadRepositoryElement} is empty");
                continue;
            }

            var serializer = GetSerializer(repositoryElement);
            if (serializer == null)
                continue;

            if (!serializer.TryDeserialize(repositoryElement.Content, out RepositoryElementContent? contentObject, out var exception) || contentObject == null)
            {
                output.Warning($"Element {variables.CurrentlyLoadRepositoryElement} failed to deserialize");
                continue;
            }

            ////TODO: if deserialization failed, we have to save this information. Otherwise changes will overwrite whole file.
            //if (!_originalRepositories.ContainsKey(repositoryLocation))
            //    _originalRepositories[repositoryLocation] = new Dictionary<string, RepositoryContent>();
            //_originalRepositories[repositoryLocation][element.Id] = contentObject;

            if (contentObject.Variables != null)
                variables.Add(repositoryLocation, contentObject.Variables, repositoryElement.Id);
            if (contentObject.Commands != null)
                commands.Add(contentObject.Commands.Where(c => c != null)!);
        }

        variables.CurrentlyLoadRepositoryElement = null;
    }

    /// <summary>
    /// Gets serializer depending on repository element format.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private ISerializer? GetSerializer(RepositoryElement element)
    {
        if (element.Format == null)
        {
            output.Warning($"Failed to determine format of {variables.CurrentlyLoadRepositoryElement}");
            return null;
        }
        var serializer = serializerFactory.GetSerializer(element.Format);
        if (serializer == null)
        {
            output.Warning($"Failed to deserialize {variables.CurrentlyLoadRepositoryElement} - unknown format {element.Format}");
            return null;
        }

        return serializer;
    }
}

