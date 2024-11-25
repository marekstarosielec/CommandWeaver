using System.Text.Json;

/// <summary>
/// Service responsible for loading variables and commands from repository (e.g. from files).
/// </summary>
public interface ILoader
{
    /// <summary>
    /// Load variables and commands from repository.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Execute(CancellationToken cancellationToken);
}

public class Loader(
    IVariableService variables, 
    IEmbeddedRepository embeddedRepository, 
    IRepository repository,
    IOutputService output, 
    IOutputSettings outputSettings,
    ICommandService iCommandService,
    IJsonSerializer serializer,
    IRepositoryElementStorage repositoryElementStorage) : ILoader
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        variables.CurrentlyLoadRepository = "built-in";
        var elements = embeddedRepository.GetList(cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.BuiltIn, null, elements);
        outputSettings.SetStyles(variables.ReadVariableValue(new DynamicValue("{{ styles }}")));

        variables.CurrentlyLoadRepository = repository.GetPath(RepositoryLocation.Application);
        elements = repository.GetList(RepositoryLocation.Application, null, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Application, null, elements);
        outputSettings.SetStyles(variables.ReadVariableValue(new DynamicValue("{{ styles }}")));

        variables.CurrentlyLoadRepository = repository.GetPath(RepositoryLocation.Session, variables.CurrentSessionName);
        elements = repository.GetList(RepositoryLocation.Session, variables.CurrentSessionName, cancellationToken);
        await LoadRepositoryElements(RepositoryLocation.Session, variables.CurrentSessionName, elements);
        variables.CurrentlyLoadRepository = null;
        outputSettings.SetStyles(variables.ReadVariableValue(new DynamicValue("{{ styles }}")));

        variables.WriteVariableValue(VariableScope.Command, "LocalPath", new DynamicValue(repository.GetPath(RepositoryLocation.Application)));
        variables.WriteVariableValue(VariableScope.Command, "SessionPath", new DynamicValue(repository.GetPath(RepositoryLocation.Session, variables.CurrentSessionName)));
        
        outputSettings.Serializer = serializer;
    }
    
    /// <summary>
    /// Loads commands and variables from repository.
    /// </summary>
    /// <param name="repositoryLocation"></param>
    /// <param name="sessionName"></param>
    /// <param name="repositoryElementsSerialized"></param>
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

            if (!string.Equals(serializer.Extension, repositoryElementSerialized.Format, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!serializer.TryDeserialize(repositoryElementSerialized.Content, out RepositoryElementContent? repositoryContent, out var exception) || repositoryContent == null)
            {
                //Still save information about repository, to avoid overriding it with partial content.
                repositoryElementStorage.Add(new RepositoryElement(repositoryLocation, repositoryElementSerialized.Id, repositoryContent));

                output.Warning($"Element {variables.CurrentlyLoadRepositoryElement} failed to deserialize");
                continue;
            }

            repositoryElementStorage.Add(new RepositoryElement(repositoryLocation, repositoryElementSerialized.Id, repositoryContent));

            if (repositoryContent.Variables != null)
                variables.Add(repositoryLocation, repositoryContent.Variables, repositoryElementSerialized.Id);
            if (repositoryContent.Commands != null)
            {
                var allCommands = repositoryContent.Commands.Where(c => c != null)!.ToList();
                iCommandService.Add(allCommands);

                //Add information about command into variables, so that they can be part of commands.
                using var doc = JsonDocument.Parse(repositoryElementSerialized.Content);
                var root = doc.RootElement.GetProperty("commands");
                if (root.ValueKind == JsonValueKind.Array)
                {
                    for (var x = 0; x < allCommands.Count(); x++)
                    {
                        var command = allCommands[x];
                        var serializedCommand = root[x].GetRawText();
                        var commandInformation = new Dictionary<string, DynamicValue?>();
                        commandInformation["key"] = new DynamicValue(command.Name);
                        commandInformation["name"] = new DynamicValue(command.Name);
                        commandInformation["description"] = new DynamicValue(command.Description);
                        commandInformation["repositoryElementId"] = new DynamicValue(repositoryElementSerialized.Id);
                        commandInformation["json"] = new DynamicValue(serializedCommand, true);
                        commandInformation["id"] = new DynamicValue(repositoryElementSerialized.Id);

                        var commandParameterValues = new List<DynamicValue>();
                        foreach (var commandParameter in command.Parameters)
                        {
                            var commandParameterValue = new Dictionary<string, DynamicValue?>();
                            commandParameterValue["key"] = new DynamicValue(commandParameter.Key);
                            commandParameterValue["description"] = new DynamicValue(commandParameter.Description);
                            commandParameterValues.Add(new DynamicValue(commandParameterValue));
                        }

                        commandInformation["parameters"] = new DynamicValue(commandParameterValues);
                        variables.WriteVariableValue(VariableScope.Command, $"commands[{command.Name}]", new DynamicValue(new DynamicValueObject(commandInformation)));
                    }
                }
            }
        }

        variables.CurrentlyLoadRepositoryElement = null;
    }
}

