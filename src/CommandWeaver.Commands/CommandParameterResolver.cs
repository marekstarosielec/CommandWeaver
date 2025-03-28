/// <summary>
/// Service responsible for resolving and preparing command parameters from provided arguments.
/// </summary>
public interface ICommandParameterResolver
{
    /// <summary>
    /// Returns list of all command parameters, resolved from dynamic value. Including built-in.
    /// </summary>
    /// <param name="command"></param>
    List<CommandParameter> GetCommandParameters(Command command);
    
    /// <summary>
    /// Prepares command parameters by resolving their values from the provided arguments.
    /// </summary>
    /// <param name="command">The command whose parameters need to be prepared.</param>
    /// <param name="arguments">A dictionary of argument key-value pairs.</param>
    void PrepareCommandParameters(Command command, Dictionary<string, string> arguments);

    /// <summary>
    /// Return CommandParameter as DynamicValue.
    /// </summary>
    /// <param name="commandParameter"></param>
    /// <returns></returns>
    DynamicValue GetCommandParameterAsDynamicValue(CommandParameter commandParameter);
}

/// <inheritdoc />
public class CommandParameterResolver(
    IOutputService outputService,
    IVariableService variableService,
    IValidationService validationService) : ICommandParameterResolver
{
    /// <inheritdoc />
    public List<CommandParameter> GetCommandParameters(Command command)
        => command.Parameters.Select(ResolveCommandParameter).Union(BuiltInCommandParameters.List).ToList();

    /// <inheritdoc />
    public void PrepareCommandParameters(Command command, Dictionary<string, string> arguments)
    {
        outputService.Trace($"Preparing parameters for command: {command.Name}");
        var allParameters = GetCommandParameters(command);
        var knownKeys = allParameters.Where(p => p.Enabled).SelectMany(p => p.GetAllNames()).ToHashSet(StringComparer.OrdinalIgnoreCase);
    
        // Detect unknown arguments
        var unknownArguments = arguments.Keys
            .Where(k => !knownKeys.Contains(k, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (unknownArguments.Any())
            throw new CommandWeaverException($"Unknown arguments: {string.Join(", ", unknownArguments)}");

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

    private CommandParameter ResolveCommandParameter(DynamicValue dynamicCommandParameter)
    {
        var currentDynamicCommandParameter = dynamicCommandParameter;
        var fromVariable = currentDynamicCommandParameter.ObjectValue?["fromVariable"]?.TextValue;
        while (fromVariable != null)
        {
            currentDynamicCommandParameter = variableService.ReadVariableValue(new DynamicValue(fromVariable), true);
            fromVariable = currentDynamicCommandParameter.ObjectValue?["fromVariable"]?.TextValue;
        }

        var result = currentDynamicCommandParameter.GetAsObject<CommandParameter>();
        if (result == null)
            throw new CommandWeaverException("Failed to resolve parameter.");

        return result;
    }

    /// <inheritdoc />
    public DynamicValue GetCommandParameterAsDynamicValue(CommandParameter commandParameter)
    {
        var result = new Dictionary<string, DynamicValue?>();
        result["name"] = commandParameter.Name;
        result["description"] = new (commandParameter.Description);
        var validation = new Dictionary<string, DynamicValue?>();
        validation["required"] = new DynamicValue(commandParameter.Validation?.Required ?? false);
        validation["allowedType"] = new DynamicValue(commandParameter.Validation?.AllowedType);
        validation["list"] = new DynamicValue(commandParameter.Validation?.List ?? false);
        //TODO: add other validation values. Add test to check if everything is serialized.
        result["validation"] = new (validation);
        result["ifNull"] = commandParameter.IfNull;
        return new DynamicValue(new DynamicValueObject(result));
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
                outputService.Trace($"Argument resolved using name '{name}': {argumentValue}");
                break;
            }

        return argumentValue;
    }
}
