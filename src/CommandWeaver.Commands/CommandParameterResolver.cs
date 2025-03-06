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
    IOutputService outputService,
    IVariableService variableService,
    IValidationService validationService,
    IFlowService flowService) : ICommandParameterResolver
{
    /// <inheritdoc />
    public void PrepareCommandParameters(Command command, Dictionary<string, string> arguments)
    {
        outputService.Trace($"Preparing parameters for command: {command.Name}");

        var allParameters = command.Parameters.Union(BuiltInCommandParameters.List).ToList();
        var knownKeys = allParameters.Where(p => p.Enabled).SelectMany(p => p.GetAllNames()).ToHashSet(StringComparer.OrdinalIgnoreCase);
    
        // Detect unknown arguments
        var unknownArguments = arguments.Keys
            .Where(k => !knownKeys.Contains(k, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (unknownArguments.Any())
        {
            flowService.Terminate($"Unknown arguments: {string.Join(", ", unknownArguments)}");
            throw new InvalidOperationException(
                $"Command '{command.Name}' received unknown arguments: {string.Join(", ", unknownArguments)}");
        }

        // Convert command parameters (both defined by command and built-in) to variables with values from arguments.
        foreach (var parameter in allParameters)
        {
            if (!parameter.Enabled)
                continue;

            var resolvedValue = ResolveArgument(parameter, arguments);
            variableService.WriteVariableValue(VariableScope.Command, parameter.Key, resolvedValue);
        }

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
        argumentValue = GetIfNullValue(argumentValue, parameter.IfNull);

        // if (argumentValue == null && parameter.Prompt?.Enabled != false)
        // {
        //     var i = new InputInformation
        //     {
        //         Message = parameter.Prompt?.Message ?? parameter.Key,
        //         PromptStyle = parameter.Prompt?.PromptStyle,
        //         Required = parameter.Required,
        //         IsSecret = parameter.Prompt?.IsSecret ?? false,
        //     };
        //     var t = inputService.Prompt(i);
        // }
        
        var resolvedValue = new DynamicValue(argumentValue);
        validationService.Validate(parameter.Validation, resolvedValue, parameter.Key);
        outputService.Trace($"Argument for parameter '{parameter.Key}' resolved successfully.");
        
        return resolvedValue;
    }

    /// <summary>
    /// Replaces argument with alternative value if user did not provide input.
    /// </summary>
    /// <param name="currentArgumentValue"></param>
    /// <param name="ifNull"></param>
    /// <returns></returns>
    private string? GetIfNullValue(string? currentArgumentValue, DynamicValue ifNull) =>
        currentArgumentValue == null && ifNull.IsNull() == false
            ? variableService.ReadVariableValue(ifNull).GetTextValue()
            : currentArgumentValue;

    /// <summary>
    /// Returns value passed as argument.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    private string? GetArgumentValue(CommandParameter parameter, Dictionary<string, string> arguments)
    {
        string? argumentValue = null;
        foreach (var name in parameter.GetAllNames())
            if (arguments.TryGetValue(name, out var otherValue))
            {
                argumentValue = otherValue;
                outputService.Debug($"Argument resolved using name '{name}': {argumentValue}");
                break;
            }

        return argumentValue;
    }
}
