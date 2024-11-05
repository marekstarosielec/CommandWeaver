using Models;
using Models.Interfaces.Context;

namespace Cli2Context;

/// <summary>
/// Reads context variables value from different repository locations, using the provided
/// <see cref="ContextVariableStorage"/> to access built-in, local, session, and changes lists.
/// </summary>
internal class ContextVariableReader(IContext context, ContextVariableStorage variableStorage)
{
    /// <summary>
    /// Reads the given variable value, attempting to interpret any text or embedded objects that might reference other variables.
    /// </summary>
    /// <param name="variableValue">The variable value to resolve.</param>
    /// <param name="treatTextValueAsVariable">If true, treats text value as variable name for resolution.</param>
    /// <returns>The resolved <see cref="VariableValue"/> or null if unresolved.</returns>
    public VariableValue? ReadVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable = false)
        => ReadVariableValue(variableValue, treatTextValueAsVariable, 0);

    /// <summary>
    /// Contains additional parameter for counting variables resolving depth. It allows to avoid StackOverflowException in case of self-referencing variable.
    /// </summary>
    /// <param name="variableValue">The variable value to resolve.</param>
    /// <param name="treatTextValueAsVariable">If true, treats text values as potential variable name for resolution.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns></returns>
    private VariableValue? ReadVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable, int depth)
    {
        if (variableValue == null)
            return null;

        depth++;
        if (depth > 50)
        {
            context.Terminate("Too deep variable resolving. Make sure that you have no circular reference.");
            return variableValue;
        }
        var result = variableValue with { };

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
    private VariableValue? ReadTextKey(string key, bool treatTextValueAsVariable, int depth)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var resolvedKey = treatTextValueAsVariable ? VariableValuePath.GetWholePathAsVariable(key) : key;
        var variableName = VariableValuePath.ExtractVariableBetweenDelimiters(resolvedKey);

        if (variableName == null)
            return new VariableValue(key);

        var resolvedVariable = ResolveSingleValue(variableName);

        if (resolvedVariable == null)
            return new VariableValue();

        if (VariableValuePath.WholePathIsSingleVariable(resolvedKey, variableName))
            //If whole key is variable name, it can be replaced by any type.
            return ReadVariableValue(resolvedVariable, false, depth);

        if (resolvedVariable.TextValue != null)
        {
            //If variable name is just part of text, it can be replaced only by text.
            resolvedKey = VariableValuePath.ReplaceVariableWithValue(resolvedKey, variableName, resolvedVariable.TextValue);
            return ReadVariableValue(new VariableValue(resolvedKey), false, depth);
        }

        context.Terminate($"{{{{ {variableName} }}}} resolved to a non-text value, it cannot be part of text.");
        return null;
    }

    /// <summary>
    /// Reads a variable object by recursively resolving its properties.
    /// </summary>
    /// <param name="key">The variable object with properties containing variable tags.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns>The resolved object with resolved properties.</returns>
    private VariableValue? ReadObjectKey(VariableValueObject key, int depth)
    {
        if (key == null)
            return null;

        var result = new Dictionary<string, VariableValue?>();

        foreach (var keyProperty in key.Keys)
            result[keyProperty] = ReadVariableValue(key[keyProperty], false, depth);

        return new VariableValue(new VariableValueObject(result));
    }

    /// <summary>
    /// Read a variable list by recursively resolving each element's properties.
    /// </summary>
    /// <param name="key">The list containing elements with properties that might have variable tags.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns>The resolved list.</returns>
    private VariableValue? ReadListKey(VariableValueList key, int depth)
    {
        if (key == null)
            return null;

        var result = new List<VariableValueObject>();

        foreach (var listElement in key)
        {
            var resolvedElement = new Dictionary<string, VariableValue?>();
            foreach (var keyProperty in listElement.Keys)
                resolvedElement[keyProperty] = ReadVariableValue(listElement[keyProperty], false, depth);

            result.Add(new VariableValueObject(resolvedElement));
        }

        return new VariableValue(new VariableValueList(result));
    }

    /// <summary>
    /// Resolves a single variable value by searching across repository locations in the order: 
    /// Changes, Session, Local, then BuiltIn.
    /// </summary>
    /// <param name="variableName">The name of the variable to resolve.</param>
    /// <returns>The resolved variable value, or null if the variable is not found.</returns>
    internal VariableValue? ResolveSingleValue(string variableName)
    {
        var builtIn = ResolveSingleValueFromSingleList(variableStorage.BuiltIn, variableName);
        var local = ResolveSingleValueFromSingleList(variableStorage.Local, variableName);
        var session = ResolveSingleValueFromSingleList(variableStorage.Session, variableName);
        var changes = ResolveSingleValueFromSingleList(variableStorage.Changes, variableName);

        if (VariableValuePath.PathIsTopLevel(variableName) &&
            (builtIn?.ListValue != null
            || local?.ListValue != null
            || session?.ListValue != null
            || changes?.ListValue != null))
        {
            // If the whole variable value is requested and it is a list, values from all locations will be combined.
            var result = new List<VariableValueObject>();

            if (changes?.ListValue != null)
                foreach (var item in changes.ListValue)
                    result.Add(item);

            if (session?.ListValue != null)
                foreach (var item in session.ListValue)
                    if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
                        result.Add(item);

            if (local?.ListValue != null)
                foreach (var item in local.ListValue)
                    if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
                        result.Add(item);

            if (builtIn?.ListValue != null)
                foreach (var item in builtIn.ListValue)
                    if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
                        result.Add(item);

            return new VariableValue { ListValue = new VariableValueList(result) };
        }

        return changes ?? session ?? local ?? builtIn;
    }

    /// <summary>
    /// Finds the value of a variable by key within a list of variables. 
    /// Supports nested access using keys with properties or list indices.
    /// </summary>
    /// <param name="variables">The list of variables to search within.</param>
    /// <param name="key">The full key for the variable, in the form variable.property[listIndex].subProperty.</param>
    /// <returns>The resolved <see cref="VariableValue"/> or null if the key is not found.</returns>
    private VariableValue? ResolveSingleValueFromSingleList(IEnumerable<Variable> variables, string key)
    {
        VariableValue? result = null;
        var pathSections = VariableValuePath.GetPathSections(key);

        for (int i = 0; i < pathSections.Count; i++)
        {
            if (i == 0 && pathSections[i].Groups[1].Success)
                result = variables.FirstOrDefault(v => v.Key.Equals(pathSections[i].Groups[1].Value))?.Value;
            else if (i == 0 && pathSections[i].Groups[2].Success)
            {
                // Key cannot start with an index
                context.Terminate($"Invalid key {key}");
                return null;
            }
            else if (!pathSections[i].Groups[1].Success && !pathSections[i].Groups[2].Success)
            {
                // Invalid element in key
                context.Terminate($"Invalid key {key}");
                return null;
            }
            else if (i > 0 && pathSections[i].Groups[1].Success)
                result = result?.ObjectValue?[pathSections[i].Groups[1].Value];
            else if (i > 0 && pathSections[i].Groups[2].Success)
                result = new VariableValue(result?.ListValue?.FirstOrDefault(v => v["key"].TextValue?.Equals(pathSections[i].Groups[2].Value) == true));

            if (result == null)
                return null;
        }

        return result;
    }
}
