
using System.Collections.Immutable;

/// <inheritdoc />
public class CommandService(IOutputService output, IFlowService flow, IConditionsService conditionsService, IVariableService variables, IRepositoryElementStorage repositoryElementStorage, IOutputSettings outputSettings) : ICommandService
{
    private List<Command> _commands = [];

    /// <inheritdoc />
    public void Add(IEnumerable<Command> commands)
    {
        _commands.AddRange(commands);
    }

    public Command? Get(string name) => _commands.FirstOrDefault(c => c.Name == name || c.OtherNames?.Any(n => n == name) == true);

    public void PrepareCommandParameters(Command command, Dictionary<string, string> arguments)
    {
        //Convert command parameters (both defined by command and built-in) to variables with values from arguments.
        foreach (var parameter in command.Parameters.Union(BuiltInCommandParameters.List))
        {
            arguments.TryGetValue(parameter.Key, out var argumentValue);
            if (argumentValue == null && parameter.OtherNames != null)
                foreach(var otherName in parameter.OtherNames)
                {
                    arguments.TryGetValue(otherName, out var otherValue);
                    if (otherValue != null)
                    {
                        argumentValue = otherValue;
                        break;
                    }
                }

            if (parameter.AllowedValues != null && argumentValue != null && !parameter.AllowedValues.Contains(argumentValue))
                flow.Terminate($"Argument {parameter.Key} has invalid value.");

            if (parameter.AllowedEnumValues != null && argumentValue != null && !Enum.GetNames(parameter.AllowedEnumValues).Any(name => name.Equals(argumentValue, StringComparison.OrdinalIgnoreCase)))
                flow.Terminate($"Argument {parameter.Key} has invalid value.");

            variables.WriteVariableValue(VariableScope.Command, parameter.Key, new DynamicValue(argumentValue));
        }

        //Check if all required parameters have value
        foreach (var parameter in command.Parameters)
        {
            var variable = variables.FindVariable(parameter.Key);
            var value = variable == null ? null : variables.ReadVariableValue(variable.Value);
            if (parameter.Required && string.IsNullOrWhiteSpace(value?.TextValue))
            {
                flow.Terminate($"Parameter {parameter.Key} requires value.");
                return;
            }
        }

        outputSettings.CurrentLogLevel = variables.LogLevel;
    }

    public void Validate()
    {
        var allNames = new List<KeyValuePair<string, string>>();
        foreach (var repositoryElement in repositoryElementStorage.Get())
            if (repositoryElement.Content?.Commands != null)
                foreach (var command in repositoryElement.Content.Commands)
                {
                    //Find commands where name is not defined.
                    if (string.IsNullOrWhiteSpace(command.Name))
                    {
                        output.Warning($"There is a command with missing name in {repositoryElement.Id}");
                        continue;
                    }
                    allNames.Add(new KeyValuePair<string, string>(command.Name, repositoryElement.Id));
                    if (command.OtherNames != null)
                        foreach (var otherName in command.OtherNames)
                            allNames.Add(new KeyValuePair<string, string>(otherName, repositoryElement.Id));

                    //Find duplicated command parameters within command.
                    var duplicatedParameters = command.Parameters.GroupBy(p => p.Key).Where(g => g.Count() > 1).Select(p => p.Key).ToList();
                    foreach (var duplicatedParameter in duplicatedParameters)
                        output.Warning($"More than one parameter named {duplicatedParameter} is defined in command {command.Name} in {repositoryElement.Id}");


                }

        //Find duplicated command names in all repositories.
        var duplicates = allNames
            .GroupBy(pair => pair.Key)
            .Where(g => g.Count() > 1)
            .ToDictionary(
                g => g.Key,
                g => string.Join(", ", g.Select(pair => pair.Value))
            );

        foreach (var duplicate in duplicates)
            output.Warning($"Command name {duplicate.Key} is duplicated in {duplicate.Value}");

    }

    public async Task ExecuteOperations(List<Operation> operations, CancellationToken cancellationToken)
    {
        foreach (var operation in operations)
            if (!conditionsService.ConditionsAreMet(operation.Conditions)
                && !cancellationToken.IsCancellationRequested)
                await ExecuteOperation(operation, cancellationToken);
    }

    internal async Task ExecuteOperation(Operation operation, CancellationToken cancellationToken)
    {
        var preparedOperation = PrepareOperationParameters(operation);
        await preparedOperation.Run(cancellationToken);
    }

    private Operation PrepareOperationParameters(Operation operation)
    {
        var parameters = operation.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        foreach (var parameterKey in operation.Parameters.Keys)
        {
            //Evaluate all operation parameters.
            parameters[parameterKey] = operation.Parameters[parameterKey] with { Value = variables.ReadVariableValue(operation.Parameters[parameterKey].OriginalValue) ?? new DynamicValue() };
            if (parameters[parameterKey].Required && parameters[parameterKey].Value.IsNull())
                flow.Terminate($"Parameter {parameterKey} is required in operation {operation.Name}.");
            if (parameters[parameterKey].RequiredText && string.IsNullOrWhiteSpace(parameters[parameterKey].Value.TextValue))
                flow.Terminate($"Parameter {parameterKey} requires text value in operation {operation.Name}.");
            if (parameters[parameterKey].RequiredList && parameters[parameterKey].Value.ListValue == null)
                flow.Terminate($"Parameter {parameterKey} requires list value in operation {operation.Name}.");

            if (parameters[parameterKey].AllowedEnumValues != null)
            {
                //TODO: This should be checked by unit test.
                if (!parameters[parameterKey].AllowedEnumValues!.IsEnum)
                    flow.Terminate($"Parameter {parameterKey} contains invalid AllowedEnumValues in operation {operation.Name}.");

                if (!string.IsNullOrWhiteSpace(parameters[parameterKey].Value.TextValue) && !Enum.GetNames(parameters[parameterKey].AllowedEnumValues!).Any(name => string.Equals(name, parameters[parameterKey].Value.TextValue!, StringComparison.OrdinalIgnoreCase)))
                    flow.Terminate($"Parameter {parameterKey} has invalid value in operation {operation.Name}.");
            }
            if (parameters[parameterKey].AllowedValues != null)
            {
                if (!string.IsNullOrWhiteSpace(parameters[parameterKey].Value.TextValue) && !parameters[parameterKey].AllowedValues!.Any(v => string.Equals(parameters[parameterKey].Value.TextValue!,v, StringComparison.OrdinalIgnoreCase)))
                    flow.Terminate($"Parameter {parameterKey} has invalid value in operation {operation.Name}.");
            }
        }
        return operation with { Parameters = parameters.ToImmutableDictionary() };
    }
}

