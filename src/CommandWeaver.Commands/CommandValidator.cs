/// <summary>
/// Interface defining validation services for commands and parameters.
/// </summary>
public interface ICommandValidator
{
    /// <summary>
    /// Validates all commands within the given repository elements.
    /// </summary>
    /// <param name="repositoryElements">The repository elements containing commands to validate.</param>
    void ValidateCommands(IEnumerable<RepositoryElement> repositoryElements);
}


/// <inheritdoc />
public class CommandValidator(IOutputService outputService) : ICommandValidator
{
    /// <inheritdoc />
    public void ValidateCommands(IEnumerable<RepositoryElement> repositoryElements)
    {
        outputService.Trace("Starting validation of commands.");

        var allNames = new List<KeyValuePair<string, string>>();

        foreach (var repositoryElement in repositoryElements)
            ValidateCommandsInRepository(repositoryElement, allNames);
        
        ValidateDuplicateNames(allNames);

        outputService.Debug("Validation of commands completed.");
    }

    /// <summary>
    /// Validates all commands within a single repository element.
    /// </summary>
    /// <param name="repositoryElement">The repository element to validate.</param>
    /// <param name="allNames">A collection to store all command names for duplicate validation.</param>
    private void ValidateCommandsInRepository(RepositoryElement repositoryElement, List<KeyValuePair<string, string>> allNames)
    {
        if (repositoryElement.Content?.Commands == null)
            return;

        foreach (var command in repositoryElement.Content.Commands)
        {
            if (string.IsNullOrWhiteSpace(command?.Name))
            {
                outputService.Warning($"There is a command with a missing name in {repositoryElement.Id}");
                continue;
            }

            allNames.Add(new KeyValuePair<string, string>(command.Name, repositoryElement.Id));

            if (command.OtherNames != null)
                foreach (var otherName in command.OtherNames)
                    allNames.Add(new KeyValuePair<string, string>(otherName, repositoryElement.Id));

            ValidateDuplicateParameters(command, repositoryElement.Id);
        }
    }

    /// <summary>
    /// Validates duplicate parameters within a single command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <param name="repositoryId">The identifier of the repository containing the command.</param>
    private void ValidateDuplicateParameters(Command command, string repositoryId)
    {
        var duplicatedParameters = command.Parameters
            .GroupBy(p => p.Key)
            .Where(g => g.Count() > 1)
            .Select(p => p.Key)
            .ToList();

        foreach (var duplicatedParameter in duplicatedParameters)
            outputService.Warning($"More than one parameter named {duplicatedParameter} is defined in command {command.Name} in {repositoryId}");
    }

    /// <summary>
    /// Validates duplicate command names across all repository elements.
    /// </summary>
    /// <param name="allNames">The collection of all command names with their corresponding repository identifiers.</param>
    private void ValidateDuplicateNames(List<KeyValuePair<string, string>> allNames)
    {
        var duplicates = allNames
            .GroupBy(pair => pair.Key)
            .Where(g => g.Count() > 1)
            .ToDictionary(
                g => g.Key,
                g => string.Join(", ", g.Select(pair => pair.Value))
            );

        foreach (var duplicate in duplicates)
            outputService.Warning($"Command name {duplicate.Key} is duplicated in {duplicate.Value}");
    }
}
