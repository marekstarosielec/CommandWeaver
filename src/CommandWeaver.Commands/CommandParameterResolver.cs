/// <summary>
/// Service responsible for resolving and preparing command parameters from provided arguments.
/// </summary>
public interface ICommandParameterResolver
{
    /// <summary>
    /// Prepares command parameters by resolving their values from the provided arguments.
    /// </summary>
    /// <param name="command">The command whose parameters need to be prepared.</param>
    /// <param name="arguments">A dictionary of argument key-value pairs.</param>
    void PrepareCommandParameters(Command command, Dictionary<string, string> arguments);
}

/// <inheritdoc />
public class CommandParameterResolver(
    IFlowService flowService,
    IOutputService outputService,
    IVariableService variableService,
    IOutputSettings outputSettings) : ICommandParameterResolver
{
    /// <inheritdoc />
    public void PrepareCommandParameters(Command command, Dictionary<string, string> arguments)
    {
        outputService.Trace($"Preparing parameters for command: {command.Name}");

        // Convert command parameters (both defined by command and built-in) to variables with values from arguments.
        foreach (var parameter in command.Parameters.Union(BuiltInCommandParameters.List))
        {
            var resolvedValue = ResolveArgument(parameter, arguments);
            variableService.WriteVariableValue(VariableScope.Command, parameter.Key, resolvedValue);
        }
        
        outputSettings.CurrentLogLevel = variableService.LogLevel;
        outputService.Trace($"Parameters for command '{command.Name}' prepared successfully.");
    }
    
    /// <summary>
    /// Resolves the argument for a given command parameter by validating and checking constraints.
    /// </summary>
    /// <param name="parameter">The command parameter being resolved.</param>
    /// <param name="arguments">The dictionary of input arguments.</param>
    /// <returns>The resolved value as a <see cref="DynamicValue"/>.</returns>
    private DynamicValue ResolveArgument(CommandParameter parameter, Dictionary<string, string> arguments)
    {
        outputService.Trace($"Resolving argument for parameter: {parameter.Key}");

        // Try to get the argument value from the main key
        var argumentValue = GetArgumentValue(parameter, arguments);

        Validate(parameter, argumentValue);
        outputService.Trace($"Argument for parameter '{parameter.Key}' resolved successfully.");
        
        return new DynamicValue(argumentValue);
    }

    /// <summary>
    /// Returns value passed as argument.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    private string? GetArgumentValue(CommandParameter parameter, Dictionary<string, string> arguments)
    {
        arguments.TryGetValue(parameter.Key, out var argumentValue);
        outputService.Debug(argumentValue != null
            ? $"Argument found for key '{parameter.Key}': {argumentValue}"
            : $"Argument not found for key '{parameter.Key}'. Checking alternative names.");

        // Check alternative names if the main key is not found
        if (argumentValue == null && parameter.OtherNames != null)
            foreach (var otherName in parameter.OtherNames)
                if (arguments.TryGetValue(otherName, out var otherValue))
                {
                    argumentValue = otherValue;
                    outputService.Debug($"Argument resolved using alternative name '{otherName}': {argumentValue}");
                    break;
                }

        return argumentValue;
    }

    /// <summary>
    /// Checks if parameter meets constraints.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="argumentValue"></param>
    private void Validate(CommandParameter parameter, string? argumentValue)
    {
        if (parameter.Required && string.IsNullOrWhiteSpace(argumentValue))
            flowService.Terminate($"Parameter {parameter.Key} requires a value.");
        
        // Validate against AllowedValues if specified
        if (parameter.AllowedValues != null && argumentValue != null && !parameter.AllowedValues.Contains(argumentValue))
        {
            outputService.Warning($"Invalid value for argument '{parameter.Key}'. Allowed values: {string.Join(", ", parameter.AllowedValues)}.");
            flowService.Terminate($"Argument '{parameter.Key}' has an invalid value.");
        }

        // Validate against AllowedEnumValues if specified
        if (parameter.AllowedEnumValues != null && argumentValue != null &&
            !Enum.GetNames(parameter.AllowedEnumValues).Any(name => name.Equals(argumentValue, StringComparison.OrdinalIgnoreCase)))
        {
            outputService.Warning($"Invalid value for argument '{parameter.Key}'. Allowed enum values: {string.Join(", ", Enum.GetNames(parameter.AllowedEnumValues))}.");
            flowService.Terminate($"Argument '{parameter.Key}' has an invalid value.");
        }
    }
}
