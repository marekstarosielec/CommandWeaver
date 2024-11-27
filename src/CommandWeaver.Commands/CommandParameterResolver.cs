/// <summary>
/// Service responsible for resolving arguments for command parameters.
/// </summary>
public interface ICommandParameterResolver
{
    /// <summary>
    /// Resolves an argument for a command parameter, validating constraints and resolving alternate names.
    /// </summary>
    /// <param name="parameter">The command parameter being resolved.</param>
    /// <param name="arguments">The dictionary of input arguments.</param>
    /// <param name="flowService">The flow service used for handling validation failures.</param>
    /// <param name="outputService">The service for logging output messages.</param>
    /// <returns>The resolved value as a <see cref="DynamicValue"/>.</returns>
    DynamicValue ResolveArgument(
        CommandParameter parameter,
        Dictionary<string, string> arguments,
        IFlowService flowService,
        IOutputService outputService);
}

/// <inheritdoc />
public class CommandParameterResolver : ICommandParameterResolver
{
    /// <inheritdoc />
    public DynamicValue ResolveArgument(
        CommandParameter parameter,
        Dictionary<string, string> arguments,
        IFlowService flowService,
        IOutputService outputService)
    {
        outputService.Trace($"Resolving argument for parameter: {parameter.Key}");

        // Try to get the argument value from the main key
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
        
        // Validate against AllowedValues if specified
        if (parameter.AllowedValues != null && argumentValue != null && !parameter.AllowedValues.Contains(argumentValue))
            flowService.Terminate($"Argument '{parameter.Key}' has an invalid value.");
        
        // Validate against AllowedEnumValues if specified
        if (parameter.AllowedEnumValues != null && argumentValue != null &&
            !Enum.GetNames(parameter.AllowedEnumValues).Any(name => name.Equals(argumentValue, StringComparison.OrdinalIgnoreCase)))
            flowService.Terminate($"Argument '{parameter.Key}' has an invalid value.");
        
        outputService.Trace($"Argument for parameter '{parameter.Key}' resolved successfully.");
        return new DynamicValue(argumentValue);
    }
}
