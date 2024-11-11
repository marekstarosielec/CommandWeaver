﻿public interface ILoader
{
    Task Execute(CancellationToken cancellationToken);
}

public class Loader(
    IVariables variables, 
    IEmbeddedRepository embeddedRepository, 
    IRepository repository,
    IOutput output, 
    ICommands commands,
    ISerializerFactory serializerFactory,
    IRepositoryStorage repositoryStorage) : ILoader
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
        var elements = embeddedRepository.GetList(cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.BuiltIn, null, elements);
        variables.CurrentlyLoadRepository = "application";
        elements = repository.GetList(RepositoryLocation.Application, null, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Application, null, elements);
        variables.CurrentlyLoadRepository = "session";
        elements = repository.GetList(RepositoryLocation.Application, null, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Session, variables.CurrentSessionName, elements);
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

            if (!serializer.TryDeserialize(repositoryElement.Content, out RepositoryElementContent? repositoryContent, out var exception) || repositoryContent == null)
            {
                //Still save information about repository, to avoid overriding it with partial conent.
                repositoryStorage.Add(new Repository(repositoryLocation, repositoryElement.Id, repositoryContent));

                output.Warning($"Element {variables.CurrentlyLoadRepositoryElement} failed to deserialize");
                continue;
            }

            repositoryStorage.Add(new Repository(repositoryLocation, repositoryElement.Id, repositoryContent));

            if (repositoryContent.Variables != null)
                variables.Add(repositoryLocation, repositoryContent.Variables, repositoryElement.Id);
            if (repositoryContent.Commands != null)
                commands.Add(repositoryContent.Commands.Where(c => c != null)!);
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

