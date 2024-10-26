using Models;
using Models.Interfaces.Context;
using System.Text.RegularExpressions;

namespace Cli2Context;

/// <summary>
/// Resolves context variables from different repository locations, using the provided
/// <see cref="ContextVariableStorage"/> to access built-in, local, session, and changes lists.
/// </summary>
internal class ContextVariableResolver(IOutput output, ContextVariableStorage variableStorage)
{
    /// <summary>
    /// Resolves the given variable value, attempting to interpret any text or embedded objects that might reference other variables.
    /// </summary>
    /// <param name="variableValue">The variable value to resolve.</param>
    /// <param name="treatTextValueAsVariable">If true, treats text value as variable name for resolution.</param>
    /// <returns>The resolved <see cref="VariableValue"/> or null if unresolved.</returns>
    public VariableValue? ResolveVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable = false)
        => ResolveVariableValue(variableValue, treatTextValueAsVariable, 0);

    /// <summary>
    /// Contains additional parameter for counting variables resolving depth. It allows to avoid StackOverflowException in case of self-referencing variable.
    /// </summary>
    /// <param name="variableValue">The variable value to resolve.</param>
    /// <param name="treatTextValueAsVariable">If true, treats text values as potential variable name for resolution.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns></returns>
    private VariableValue? ResolveVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable, int depth)
    {
        if (variableValue == null)
            return null;

        depth++;
        if (depth > 50)
        {
            output.Error("Too deep variable resolving. Make sure that you have no circular reference.");
            return variableValue;
        }
        var result = variableValue with { };

        if (result.TextValue != null)
            result = ResolveTextKey(result.TextValue, treatTextValueAsVariable, depth) ?? variableValue;
        if (result?.ObjectValue != null)
            result = ResolveObjectKey(result.ObjectValue, depth);
        if (result?.ListValue != null)
            result = ResolveListKey(result.ListValue, depth);

        return result;
    }

    /// <summary>
    /// Resolves a text key by replacing embedded variable references with actual values.
    /// </summary>
    /// <param name="key">The text key containing potential variable tags.</param>
    /// <param name="treatTextValueAsVariable">Indicates if the text should be treated as a variable name.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns>The resolved variable value, or null if the resolution fails.</returns>
    private VariableValue? ResolveTextKey(string key, bool treatTextValueAsVariable, int depth)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var resolvedKey = treatTextValueAsVariable ? $"{{{{ {key} }}}}" : key;
        var variableName = ExtractVariableBetweenDelimiters(resolvedKey);

        if (variableName == null)
            return new VariableValue(key);

        var resolvedVariable = ResolveSingleValue(variableName);

        if (resolvedVariable == null)
            return null;

        var wholeKeyIsSingleVariable = resolvedKey.StartsWith("{{") && resolvedKey.EndsWith("}}") && resolvedKey.Trim('{', '}', ' ').Equals(variableName, StringComparison.OrdinalIgnoreCase);

        if (wholeKeyIsSingleVariable)
            //If whole key is variable name, it can be replaced by any type.
            return ResolveVariableValue(resolvedVariable, false, depth);

        if (resolvedVariable.TextValue != null)
        {
            //If variable name is just part of text, it can be replaced only by text.
            string pattern = $@"\{{\{{\s*{Regex.Escape(variableName)}\s*\}}\}}";
            resolvedKey = Regex.Replace(resolvedKey, pattern, resolvedVariable.TextValue);
            return ResolveVariableValue(new VariableValue(resolvedKey), false, depth);
        }

        output.Error($"{{{{ {variableName} }}}} resolved to a non-text value, it cannot be part of text.");
        return null;
    }

    /// <summary>
    /// Resolves a variable object by recursively resolving its properties.
    /// </summary>
    /// <param name="key">The variable object with properties containing variable tags.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns>The resolved object with resolved properties.</returns>
    private VariableValue? ResolveObjectKey(VariableValueObject key, int depth)
    {
        if (key == null)
            return null;

        var result = new Dictionary<string, VariableValue?>();

        foreach (var keyProperty in key.Keys)
            result[keyProperty] = ResolveVariableValue(key[keyProperty], false, depth);

        return new VariableValue(new VariableValueObject(result));
    }

    /// <summary>
    /// Resolves a variable list by recursively resolving each element's properties.
    /// </summary>
    /// <param name="key">The list containing elements with properties that might have variable tags.</param>
    /// <param name="depth">Current resolving depth.</param>
    /// <returns>The resolved list.</returns>
    private VariableValue? ResolveListKey(VariableValueList key, int depth)
    {
        if (key == null)
            return null;

        var result = new List<Dictionary<string, VariableValue?>>();

        foreach (var listElement in key)
        {
            var resolvedElement = new Dictionary<string, VariableValue?>();

            foreach (var keyProperty in listElement.Keys)
                resolvedElement[keyProperty] = ResolveVariableValue(listElement[keyProperty], false, depth);

            result.Add(resolvedElement);
        }

        return new VariableValue(new VariableValueList(result));
    }

    /// <summary>
    /// Resolves a single variable value by searching across repository locations in the order: 
    /// Changes, Session, Local, then BuiltIn.
    /// </summary>
    /// <param name="variableName">The name of the variable to resolve.</param>
    /// <returns>The resolved variable value, or null if the variable is not found.</returns>
    private VariableValue? ResolveSingleValue(string variableName)
    {
        var builtIn = ResolveSingleValueFromSingleList(variableStorage.BuiltIn, variableName);
        var local = ResolveSingleValueFromSingleList(variableStorage.Local, variableName);
        var session = ResolveSingleValueFromSingleList(variableStorage.Session, variableName);
        var changes = ResolveSingleValueFromSingleList(variableStorage.Changes, variableName);

        // var isList = false;
        // if (KeyIsTopLevel(key) &&
        //     (builtIn?.ListValue != null 
        //     || local?.ListValue != null
        //     || session?.ListValue != null
        //     || changes?.ListValue != null))
        //     isList = true; // If the whole variable value is requested and it is a list, values from all locations will be combined.

        // if (!isList)
        return changes ?? session ?? local ?? builtIn;
        // else
        // {
        //     // Return all rows
        //     var result = new VariableValueList();
        //     if (changes?.ListValue != null)
        //         foreach (var item in changes.ListValue)
        //             result.Add(item);

        //     if (session?.ListValue != null)
        //         foreach (var item in session.ListValue)
        //             if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
        //                 result.Add(item);

        //     if (local?.ListValue != null)
        //         foreach (var item in local.ListValue)
        //             if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
        //                 result.Add(item);

        //     if (builtIn?.ListValue != null)
        //         foreach (var item in builtIn.ListValue)
        //             if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
        //                 result.Add(item);

        //     return new VariableValue { ListValue = result };
        // }
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
        var pattern = @"([a-zA-Z0-9_\-\s]+)|\[(.*?)\]";
        var matches = Regex.Matches(key, pattern);

        for (int i = 0; i < matches.Count; i++)
        {
            if (i == 0 && matches[i].Groups[1].Success)
                result = variables.FirstOrDefault(v => v.Key.Equals(matches[i].Groups[1].Value, StringComparison.InvariantCultureIgnoreCase))?.Value;
            else if (i == 0 && matches[i].Groups[2].Success)
            {
                // Key cannot start with an index
                output.Error($"Invalid key {key}");
                return null;
            }
            else if (!matches[i].Groups[1].Success && !matches[i].Groups[2].Success)
            {
                // Invalid element in key
                output.Error($"Invalid key {key}");
                return null;
            }
            else if (i > 0 && matches[i].Groups[1].Success)
                result = result?.ObjectValue?[matches[i].Groups[1].Value];
            else if (i > 0 && matches[i].Groups[2].Success)
                result = new VariableValue(result?.ListValue?.FirstOrDefault(v => v["key"].TextValue?.Equals(matches[i].Groups[2].Value, StringComparison.InvariantCultureIgnoreCase) == true));

            if (result == null)
                return null;
        }

        return result;
    }

    /// <summary>
    /// Extracts the string between specified delimiters within a given input string.
    /// </summary>
    /// <param name="input">The input string from which to extract the variable.</param>
    /// <returns>The extracted string between the delimiters, or null if the delimiters are not found.</returns>
    private string? ExtractVariableBetweenDelimiters(string input)
    {
        int closingIndex = input.IndexOf("}}");
        if (closingIndex == -1) return null;

        int openingIndex = input.LastIndexOf("{{", closingIndex);
        if (openingIndex == -1) return null;

        return input.Substring(openingIndex + 2, closingIndex - openingIndex - 2).Trim();
    }
}
