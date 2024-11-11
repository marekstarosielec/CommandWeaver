/// <summary>
/// Defines a service for handling command definitions.
/// </summary>
public interface ICommands
{
    /// <summary>
    /// Adds a set of commands that can be executed.
    /// </summary>
    /// <param name="commands"></param>
    void Add(IEnumerable<Command> commands);

    void Validate();

    Command? Get(string name);

    void PrepareCommandParameters(Command command, Dictionary<string, string> arguments);

    Task ExecuteCommand(Command command, CancellationToken cancellationToken);
}


/// <inheritdoc />
public class Commands(IOutput output, IFlow flow, IOperationConditions operationConditions, IVariables variables) : ICommands
{
    private List<Command> _commands = [];

    /// <inheritdoc />
    public void Add(IEnumerable<Command> commands)
    {
        _commands.AddRange(commands);
    }

    public Command? Get(string name) => _commands.FirstOrDefault(c => c.Name == name);

    public void PrepareCommandParameters(Command command, Dictionary<string, string> arguments)
    {
        //Convert command parameters (both defined by command and built-in) to variables with values from arguments.
        foreach (var parameter in command.Parameters.Union(BuiltInCommandParameters.List))
        {
            arguments.TryGetValue(parameter.Key, out var argumentValue);
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
    }

    public void Validate()
    {
        var commandWithDuplicateNames = _commands
            .GroupBy(x => x.Name)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .ToList();

        foreach (var command in commandWithDuplicateNames)
            output.Warning($"Command name {command.Name} is duplicated");
    }

    public async Task ExecuteCommand(Command command, CancellationToken cancellationToken)
    {
        foreach (var operation in command.Operations)
            if (!operationConditions.OperationShouldBeSkipped(operation, variables)
                && !cancellationToken.IsCancellationRequested)
                await ExecuteOperation(operation, cancellationToken);
    }

    internal async Task ExecuteOperation(Operation operation, CancellationToken cancellationToken)
    {
        PrepareOperationParameters(operation, variables);
        await operation.Run(cancellationToken);
    }

    private void PrepareOperationParameters(Operation operation, IVariables variables)
    {
        foreach (var parameterKey in operation.Parameters.Keys)
        {
            //Evaluate all operation parametes.
            operation.Parameters[parameterKey] = operation.Parameters[parameterKey] with { Value = variables.ReadVariableValue(operation.Parameters[parameterKey].Value) ?? new DynamicValue() };
            if (operation.Parameters[parameterKey].Required && operation.Parameters[parameterKey].Value.IsNull())
                flow.Terminate($"Parameter {parameterKey} is required in operation {operation.Name}.");
            if (operation.Parameters[parameterKey].RequiredText && string.IsNullOrWhiteSpace(operation.Parameters[parameterKey].Value.TextValue))
                flow.Terminate($"Parameter {parameterKey} requires text value in operation {operation.Name}.");

            if (operation.Parameters[parameterKey].AllowedEnumValues != null)
            {
                //TODO: This should be checked by unit test.
                if (!operation.Parameters[parameterKey].AllowedEnumValues!.IsEnum)
                    flow.Terminate($"Parameter {parameterKey} contains invalid AllowedEnumValues in operation {operation.Name}.");

                if (!string.IsNullOrWhiteSpace(operation.Parameters[parameterKey].Value.TextValue) && !Enum.IsDefined(operation.Parameters[parameterKey].AllowedEnumValues!, operation.Parameters[parameterKey].Value.TextValue!))
                    flow.Terminate($"Parameter {parameterKey} has invalid value in operation {operation.Name}.");
            }
        }
    }
}

