using System.Text.RegularExpressions;

/// <summary>
/// Reads context variables value from different repository locations, using the provided
/// <see cref="VariableStorage"/> to access built-in, local, session, and changes lists.
/// </summary>
public interface IReader
{
    /// <summary>
    /// Reads the given variable value, attempting to interpret any text or embedded objects that might reference other variables.
    /// </summary>
    /// <param name="variableValue">The variable value to resolve.</param>
    /// <param name="treatTextValueAsVariable">If true, treats text value as variable name for resolution.</param>
    /// <param name="maximumDepth">Maximum depth of resolving variables.</param>
    /// <returns>The resolved <see cref="DynamicValue"/> or null if unresolved.</returns>
    DynamicValue ReadVariableValue(DynamicValue? variableValue, bool treatTextValueAsVariable = false, int maximumDepth = 50);
}

/// <inheritdoc />
public class Reader(IFlowService flowService, IOutputService outputService, IVariableStorage variableStorage) : IReader
{
    /// <inheritdoc />
    public DynamicValue ReadVariableValue(DynamicValue? variableValue, bool treatTextValueAsVariable = false, int maximumDepth = 50)
    {
        outputService.Trace($"Starting variable resolution.");
        outputService.Write(variableValue ?? new DynamicValue(), LogLevel.Trace, Styling.Raw);
        outputService.Write(new DynamicValue(Environment.NewLine), LogLevel.Trace, Styling.Raw);

        return ReadVariableValue(variableValue, treatTextValueAsVariable, 0, false, maximumDepth);
    }

    /// <summary>
    /// Contains additional parameter for counting variables resolving depth. It allows to avoid StackOverflowException in case of self-referencing variable.
    /// </summary>
    /// <param name="variableValue">The variable value to resolve.</param>
    /// <param name="treatTextValueAsVariable">If true, treats text values as potential variable name for resolution.</param>
    /// <param name="currentDepth">Current resolving depth.</param>
    /// <param name="noResolving">If reached value which has NoResolving flag set, resolving is stopped. This is useful when we have variables containing commands.</param>
    /// <param name="maximumDepth">Maximum depth of resolving variables.</param>
    /// <returns></returns>
    private DynamicValue ReadVariableValue(DynamicValue? variableValue, bool treatTextValueAsVariable, int currentDepth,
        bool noResolving, int maximumDepth)
    {
        if (variableValue == null)
        {
            outputService.Warning("Variable value is null. Returning an empty DynamicValue.");
            return new DynamicValue();
        }
       
        currentDepth++;
        if (currentDepth > maximumDepth)
            return variableValue;
        
        var result = variableValue with { };

        //Allow to stop resolving at some level, e.g. when printing command json, we don't want to have variables inside resolved.
        if (result.NoResolving || noResolving)
            return result;

        if (result.TextValue != null)
            result = ReadTextKey(result.TextValue, treatTextValueAsVariable, currentDepth, maximumDepth) ?? variableValue;

        if (result.DateTimeValue.HasValue)
            return result; // DateTime values don't require further resolution

        if (result.BoolValue.HasValue)
            return result; // Boolean values are final

        if (result.NumericValue.HasValue)
            return result; // Numeric values are final

        if (result.PrecisionValue.HasValue)
            return result; // Precision (double) values are final

        if (result.ObjectValue != null)
            result = ReadObjectKey(result.ObjectValue, currentDepth, maximumDepth);

        if (result.ListValue != null)
            result = ReadListKey(result.ListValue, currentDepth, maximumDepth);

        return result;
    }

    private DynamicValue? ReadTextKey(string key, bool treatTextValueAsVariable, int currentDepth, int maximumDepth)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var resolvedKey = treatTextValueAsVariable ? ValuePath.GetWholePathAsVariable(key) : key;
        var path = ValuePath.ExtractVariableBetweenDelimiters(resolvedKey);

        if (path == null)
            return new DynamicValue(key);

        var resolvedVariable = ResolveSingleValue(path);

        var wholePathIsSingleVariable = ValuePath.WholePathIsSingleVariable(resolvedKey, path);
        
        if (wholePathIsSingleVariable && resolvedVariable == null)
            return new DynamicValue();

        //If variable name is just part of text and is empty, replace it by e,pty text.
        if (!wholePathIsSingleVariable && resolvedVariable == null)
        {
            resolvedKey = ValuePath.ReplaceVariableWithValue(resolvedKey, path, string.Empty);
            return ReadVariableValue(new DynamicValue(resolvedKey), false, currentDepth, false, maximumDepth);
        }

        if (wholePathIsSingleVariable)
            //If whole key is variable name, it can be replaced by any type.
            return ReadVariableValue(resolvedVariable, false, currentDepth, false, maximumDepth);

        if (resolvedVariable!.TextValue != null)
        {
            //If variable name is just part of text, it can be replaced only by text.

            //If variable contains styling, it is replaced here so it is not applied.
            var replacement = resolvedVariable.TextValue.Replace("[[", "/[/[").Replace("]]", "/]/]");
            resolvedKey = ValuePath.ReplaceVariableWithValue(resolvedKey, path, replacement);
            return ReadVariableValue(new DynamicValue(resolvedKey), false, currentDepth, resolvedVariable.NoResolving, maximumDepth);
        }
        
        if (resolvedVariable.NumericValue != null)
        {
            //Handling variable that resolved to number as part of text.
            //TODO: Add possibility to define ToString format.
            //TODO: Support other DynamicValues like precision, date, etc.
            var replacement = resolvedVariable.NumericValue.ToString() ?? string.Empty;
            resolvedKey = ValuePath.ReplaceVariableWithValue(resolvedKey, path, replacement);
            return ReadVariableValue(new DynamicValue(resolvedKey), false, currentDepth, resolvedVariable.NoResolving, maximumDepth);
        }

        flowService.Terminate($"{{{{ {path} }}}} resolved to a non-text value, it cannot be part of text.");
        return null;
    }

    private DynamicValue ReadObjectKey(DynamicValueObject key, int currentDepth, int maximumDepth)
    {
        var result = new Dictionary<string, DynamicValue?>();

        foreach (var keyProperty in key.Keys)
            result[keyProperty] = ReadVariableValue(key[keyProperty], false, currentDepth, false, maximumDepth);

        return new DynamicValue(new DynamicValueObject(result));
    }

    private DynamicValue ReadListKey(DynamicValueList key, int currentDepth, int maximumDepth)
    {
        var result = new List<DynamicValue>();

        foreach (var listElement in key)
            result.Add(ReadVariableValue(listElement, false, currentDepth, false, maximumDepth));

        return new DynamicValue(result);
    }

    private DynamicValue? ResolveSingleValue(string path)
    {
        // Resolve individual storage locations
        var builtIn = ResolveSingleValueFromSingleList(variableStorage.BuiltIn, path);
        var application = ResolveSingleValueFromSingleList(variableStorage.Application, path);
        var session = ResolveSingleValueFromSingleList(variableStorage.Session, path);
        var command = ResolveSingleValueFromSingleList(variableStorage.Command, path);

        if (ValuePath.PathIsTopLevel(path) && HasAnyList(builtIn, application, session, command))
            return CombineLists(builtIn, application, session, command);

        if (command?.IsNull() == false)
            return command;
        if (session?.IsNull() == false)
            return session;
        if (application?.IsNull() == false)
            return application;
        
        return builtIn;
    }
    
    private bool HasAnyList(params DynamicValue?[] values) =>
        values.Any(value => value?.ListValue != null);
    
    private DynamicValue CombineLists(params DynamicValue?[] storages)
    {
        var combinedList = new List<DynamicValue>();

        foreach (var storage in storages)
        {
            if (storage?.ListValue == null) continue;

            foreach (var item in storage.ListValue)
            {
                // Avoid adding duplicates by checking the "key" property
                var key = item.ObjectValue?.ContainsKey("key") == true ? item.ObjectValue["key"]?.TextValue : null;
                if (key == null || combinedList.All(existing => existing.ObjectValue?["key"].TextValue != key))
                    combinedList.Add(item);
            }
        }

        return new DynamicValue(combinedList);
    }
    private DynamicValue? ResolveSingleValueFromSingleList(IEnumerable<Variable> variables, string key)
    {
        // Ensure variables are a list for efficient access
        var variablesList = variables as IList<Variable> ?? variables.ToList();
        DynamicValue? result = null;

        // Split the key into hierarchical sections
        var pathSections = ValuePath.GetPathSections(key);

        foreach (var (section, index) in pathSections.Select((section, idx) => (section, idx)))
        {
            result = index == 0 
                ? ResolveTopLevelKey(variablesList, section, key) 
                : ResolveNestedKey(result, section, key);

            if (result == null)
                return null;
        }

        return result;
    }
    
    private DynamicValue? ResolveTopLevelKey(IList<Variable> variablesList, Match section, string key)
    {
        if (section.Groups[1].Success)
            // Find the variable by its key
            return variablesList.FirstOrDefault(v => v.Key.Equals(section.Groups[1].Value))?.Value;
        
        if (section.Groups[2].Success)
        {
            flowService.Terminate($"Invalid key '{key}' - cannot start with an index.");
            return null;
        }

        flowService.Terminate($"Invalid key '{key}' - no valid groups found.");
        return null;
    }

    private DynamicValue? ResolveNestedKey(DynamicValue? currentValue, Match section, string key)
    {
        if (currentValue == null)
            return null;

        if (section.Groups[1].Success)
        {
            // Access property from object
            var propertyName = section.Groups[1].Value;
            return currentValue.ObjectValue?.ContainsKey(propertyName) == true ? currentValue.ObjectValue[propertyName] : null; // Gracefully skip invalid properties
        }

        if (section.Groups[2].Success)
        {
            // Access element from list by key
            var indexKey = section.Groups[2].Value;
            return currentValue.ListValue?
                .FirstOrDefault(v => v.ObjectValue?["key"].TextValue?.Equals(indexKey) == true);
        }

        flowService.Terminate($"Invalid section in key '{key}'");
        return null;
    }

}
