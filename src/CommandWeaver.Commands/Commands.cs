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
}


/// <inheritdoc />
public class Commands(IOutput output, IVariables variables, IFlow flow) : ICommands
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


}

