public interface ILoader
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
    IRepositoryElementStorage repositoryElementStorage) : ILoader
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

        variables.CurrentlyLoadRepository = repository.GetPath(RepositoryLocation.Application, null);
        elements = repository.GetList(RepositoryLocation.Application, null, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Application, null, elements);

        variables.CurrentlyLoadRepository = repository.GetPath(RepositoryLocation.Session, variables.CurrentSessionName);
        elements = repository.GetList(RepositoryLocation.Session, variables.CurrentSessionName, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Session, variables.CurrentSessionName, elements);
        variables.CurrentlyLoadRepository = null;

        variables.WriteVariableValue(VariableScope.Command, "LocalPath", new DynamicValue(repository.GetPath(RepositoryLocation.Application)));
        variables.WriteVariableValue(VariableScope.Command, "SessionPath", new DynamicValue(repository.GetPath(RepositoryLocation.Session, variables.CurrentSessionName)));
    }

    /// <summary>
    /// Loads commands and variables from repository.
    /// </summary>
    /// <param name="repositoryLocation"></param>
    /// <param name="sessionName"></param>
    /// <param name="repositoryElements"></param>
    /// <returns></returns>
    private async Task LoadRepositoryElements(RepositoryLocation repositoryLocation, string? sessionName, IAsyncEnumerable<RepositoryElementSerialized> repositoryElementsSerialized)
    {
        await foreach (var repositoryElementSerialized in repositoryElementsSerialized)
        {
            variables.CurrentlyLoadRepositoryElement = repositoryElementSerialized.FriendlyName;
            output.Debug($"Processing element {variables.CurrentlyLoadRepository}\\{variables.CurrentlyLoadRepositoryElement}");

            if (string.IsNullOrWhiteSpace(repositoryElementSerialized.Content))
            {
                output.Warning($"Element {variables.CurrentlyLoadRepositoryElement} is empty");
                continue;
            }

            var serializer = GetSerializer(repositoryElementSerialized);
            if (serializer == null)
                continue;

            if (!serializer.TryDeserialize(repositoryElementSerialized.Content, out RepositoryElementContent? repositoryContent, out var exception) || repositoryContent == null)
            {
                //Still save information about repository, to avoid overriding it with partial conent.
                repositoryElementStorage.Add(new RepositoryElement(repositoryLocation, repositoryElementSerialized.Id, repositoryContent));

                output.Warning($"Element {variables.CurrentlyLoadRepositoryElement} failed to deserialize");
                continue;
            }

            repositoryElementStorage.Add(new RepositoryElement(repositoryLocation, repositoryElementSerialized.Id, repositoryContent));

            var defaultSerializer = serializerFactory.GetDefaultSerializer(out _);
            if (repositoryContent.Variables != null)
                variables.Add(repositoryLocation, repositoryContent.Variables, repositoryElementSerialized.Id);
            if (repositoryContent.Commands != null)
            {
                var allCommands = repositoryContent.Commands.Where(c => c != null)!;
                commands.Add(allCommands);
                foreach (var command in allCommands)
                    if (defaultSerializer.TrySerialize(command, out var serializedSingleCommand, out _))
                    {
                        var commandInformation = new Dictionary<string, DynamicValue?>();
                        commandInformation["key"] = new DynamicValue(command.Name);
                        commandInformation["json"] = new DynamicValue(serializedSingleCommand, true);
                        variables.WriteVariableValue(VariableScope.Command, $"commands[{command.Name}]", new DynamicValue(new DynamicValueObject(commandInformation)));
                    }
                
            }
        }

        variables.CurrentlyLoadRepositoryElement = null;
    }

    /// <summary>
    /// Gets serializer depending on repository element format.
    /// </summary>
    /// <param name="repositoryElementSerialized"></param>
    /// <returns></returns>
    private ISerializer? GetSerializer(RepositoryElementSerialized repositoryElementSerialized)
    {
        if (repositoryElementSerialized.Format == null)
        {
            output.Warning($"Failed to determine format of {variables.CurrentlyLoadRepositoryElement}");
            return null;
        }
        var serializer = serializerFactory.GetSerializer(repositoryElementSerialized.Format);
        if (serializer == null)
        {
            output.Warning($"Failed to deserialize {variables.CurrentlyLoadRepositoryElement} - unknown format {repositoryElementSerialized.Format}");
            return null;
        }

        return serializer;
    }
}

