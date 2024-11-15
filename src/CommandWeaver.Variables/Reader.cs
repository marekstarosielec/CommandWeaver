/// <summary>
/// Reads context variables value from different repository locations, using the provided
/// <see cref="Storage"/> to access built-in, local, session, and changes lists.
/// </summary>
public interface IReader
{
    /// <summary>
    /// Reads the given variable value, attempting to interpret any text or embedded objects that might reference other variables.
    /// </summary>
    /// <param name="variableValue">The variable value to resolve.</param>
    /// <param name="treatTextValueAsVariable">If true, treats text value as variable name for resolution.</param>
    /// <returns>The resolved <see cref="DynamicValue"/> or null if unresolved.</returns>
    DynamicValue ReadVariableValue(DynamicValue? variableValue, bool treatTextValueAsVariable = false);
}

/// <inheritdoc />
public class Reader(IFlow flow, Storage variableStorage) : IReader
{
    /// <inheritdoc />
    public DynamicValue ReadVariableValue(DynamicValue? variableValue, bool treatTextValueAsVariable = false)
        => ReadVariableValue(variableValue, treatTextValueAsVariable, 0) ?? new DynamicValue();

    /// <summary>
    /// Contains additional parameter for counting variables resolving depth. It allows to avoid StackOverflowException in case of self-referencing variable.
    /// </summary>
    /// <param name="variableValue">The variable value to resolve.</param>
    /// <param name="treatTextValueAsVariable">If true, treats text values as potential variable name for resolution.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns></returns>
    private DynamicValue? ReadVariableValue(DynamicValue? variableValue, bool treatTextValueAsVariable, int depth)
    {
        if (variableValue == null)
            return null;

        depth++;
        if (depth > 50)
        {
            flow.Terminate("Too deep variable resolving. Make sure that you have no circular reference.");
            return variableValue;
        }
        var result = variableValue with { };
        
        //Allow to stop resolving at some level, e.g. when printing command json, we don't want to have variables inside resolved.
        if (result.NoResolving)
            return result;

        if (result.TextValue != null)
            result = ReadTextKey(result.TextValue, treatTextValueAsVariable, depth) ?? variableValue;
        if (result?.ObjectValue != null)
            result = ReadObjectKey(result.ObjectValue, depth);
        if (result?.ListValue != null)
            result = ReadListKey(result.ListValue, depth);

        return result;
    }

    /// <summary>
    /// Reads a text key by replacing embedded variable references with actual values.
    /// </summary>
    /// <param name="key">The text key containing potential variable tags.</param>
    /// <param name="treatTextValueAsVariable">Indicates if the text should be treated as a variable name.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns>The resolved variable value, or null if the resolution fails.</returns>
    private DynamicValue? ReadTextKey(string key, bool treatTextValueAsVariable, int depth)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var resolvedKey = treatTextValueAsVariable ? ValuePath.GetWholePathAsVariable(key) : key;
        var path = ValuePath.ExtractVariableBetweenDelimiters(resolvedKey);

        if (path == null)
            return new DynamicValue(key);

        var resolvedVariable = ResolveSingleValue(path);

        if (resolvedVariable == null)
            return new DynamicValue();

        if (ValuePath.WholePathIsSingleVariable(resolvedKey, path))
            //If whole key is variable name, it can be replaced by any type.
            return ReadVariableValue(resolvedVariable, false, depth);

        if (resolvedVariable.TextValue != null)
        {
            //If variable name is just part of text, it can be replaced only by text.
            resolvedKey = ValuePath.ReplaceVariableWithValue(resolvedKey, path, resolvedVariable.TextValue);
            return ReadVariableValue(new DynamicValue(resolvedKey), false, depth);
        }

        flow.Terminate($"{{{{ {path} }}}} resolved to a non-text value, it cannot be part of text.");
        return null;
    }

    /// <summary>
    /// Reads a variable object by recursively resolving its properties.
    /// </summary>
    /// <param name="key">The variable object with properties containing variable tags.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns>The resolved object with resolved properties.</returns>
    private DynamicValue? ReadObjectKey(DynamicValueObject key, int depth)
    {
        if (key == null)
            return null;

        var result = new Dictionary<string, DynamicValue?>();

        foreach (var keyProperty in key.Keys)
            result[keyProperty] = ReadVariableValue(key[keyProperty], false, depth);

        return new DynamicValue(new DynamicValueObject(result));
    }

    /// <summary>
    /// Read a variable list by recursively resolving each element's properties.
    /// </summary>
    /// <param name="key">The list containing elements with properties that might have variable tags.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns>The resolved list.</returns>
    private DynamicValue? ReadListKey(DynamicValueList key, int depth)
    {
        if (key == null)
            return null;

        var result = new List<DynamicValueObject>();

        foreach (var listElement in key)
        {
            var resolvedElement = new Dictionary<string, DynamicValue?>();
            foreach (var keyProperty in listElement.Keys)
                resolvedElement[keyProperty] = ReadVariableValue(listElement[keyProperty], false, depth);

            result.Add(new DynamicValueObject(resolvedElement));
        }

        return new DynamicValue(result);
    }

    /// <summary>
    /// Resolves a single variable value by searching across repository locations in the order: 
    /// Changes, Session, Local, then BuiltIn.
    /// </summary>
    /// <param name="path">The name of the variable to resolve.</param>
    /// <returns>The resolved variable value, or null if the variable is not found.</returns>
    internal DynamicValue? ResolveSingleValue(string path)
    {
        var builtIn = ResolveSingleValueFromSingleList(variableStorage.BuiltIn, path);
        var application = ResolveSingleValueFromSingleList(variableStorage.Application, path);
        var session = ResolveSingleValueFromSingleList(variableStorage.Session, path);
        var command = ResolveSingleValueFromSingleList(variableStorage.Command, path);

        if (ValuePath.PathIsTopLevel(path) &&
            (builtIn?.ListValue != null
            || application?.ListValue != null
            || session?.ListValue != null
            || command?.ListValue != null))
        {
            // If the whole variable value is requested and it is a list, values from all locations will be combined.
            var result = new List<DynamicValueObject>();

            if (command?.ListValue != null)
                foreach (var item in command.ListValue)
                    result.Add(item);

            if (session?.ListValue != null)
                foreach (var item in session.ListValue)
                    if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
                        result.Add(item);

            if (application?.ListValue != null)
                foreach (var item in application.ListValue)
                    if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
                        result.Add(item);

            if (builtIn?.ListValue != null)
                foreach (var item in builtIn.ListValue)
                    if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
                        result.Add(item);

            return new DynamicValue(result);
        }
        if (command?.IsNull() == false)
            return command;
        if (session?.IsNull() == false)
            return session;
        if (application?.IsNull() == false)
            return application;
        return builtIn;
    }

    /// <summary>
    /// Finds the value of a variable by key within a list of variables. 
    /// Supports nested access using keys with properties or list indices.
    /// </summary>
    /// <param name="variables">The list of variables to search within.</param>
    /// <param name="key">The full key for the variable, in the form variable.property[listIndex].subProperty.</param>
    /// <returns>The resolved <see cref="DynamicValue"/> or null if the key is not found.</returns>
    private DynamicValue? ResolveSingleValueFromSingleList(IEnumerable<Variable> variables, string key)
    {
        DynamicValue? result = null;
        var pathSections = ValuePath.GetPathSections(key);

        for (int i = 0; i < pathSections.Count; i++)
        {
            if (i == 0 && pathSections[i].Groups[1].Success)
                result = variables.FirstOrDefault(v => v.Key.Equals(pathSections[i].Groups[1].Value))?.Value;
            else if (i == 0 && pathSections[i].Groups[2].Success)
            {
                // Key cannot start with an index
                flow.Terminate($"Invalid key {key}");
                return null;
            }
            else if (!pathSections[i].Groups[1].Success && !pathSections[i].Groups[2].Success)
            {
                // Invalid element in key
                flow.Terminate($"Invalid key {key}");
                return null;
            }
            else if (i > 0 && pathSections[i].Groups[1].Success)
                result = result?.ObjectValue?[pathSections[i].Groups[1].Value];
            else if (i > 0 && pathSections[i].Groups[2].Success)
                result = new DynamicValue(result?.ListValue?.FirstOrDefault(v => v["key"].TextValue?.Equals(pathSections[i].Groups[2].Value) == true));

            if (result == null)
                return null;
        }

        return result;
    }
}
