using System.Text.RegularExpressions;
using Models;
using Models.Interfaces.Context;

namespace Cli2Context;

public class ContextVariables(IOutput output) : IContextVariables
{
    internal readonly List<Variable> _builtIn = new();
    internal readonly List<Variable> _local = new();
    internal readonly List<Variable> _session = new();
    internal readonly List<Variable> _changes = new();

    public string CurrentSessionName
    {
        get => GetValueAsString("currentSessionName", true) ?? "session1";
        set => SetVariableValue(VariableScope.Application, "currentSessionName", value);
    }

    public string? CurrentlyProcessedElement
    {
        get => GetValueAsString("currentlyProcessedElement", true);
        set => SetVariableValue(VariableScope.Command, "currentlyProcessedElement", value);
    }
    
    public void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent)
    {
        var elementsToImport = elementsWithContent.Where(v => v != null).Cast<Variable>();
        switch (repositoryLocation)
        {
            case RepositoryLocation.BuiltIn:
                _builtIn.Clear();
                foreach (var elementWithContentKey in elementsToImport)
                    _builtIn.Add(elementWithContentKey with { Scope = VariableScope.Application});
                break;
            case RepositoryLocation.Local:
                _local.Clear();
                foreach (var elementWithContentKey in elementsToImport)
                    _local.Add(elementWithContentKey with { Scope = VariableScope.Application});
                break;
            case RepositoryLocation.Session:
                _session.Clear();
                foreach (var elementWithContentKey in elementsToImport)
                    _session.Add(elementWithContentKey with { Scope = VariableScope.Session});
                break;
            default:
                throw new InvalidOperationException($"Unknown repository location: {repositoryLocation}");
        }
    }

    public VariableValue GetValue(VariableValue? variableValue, bool asVariable = false)
    {
        var result = new VariableValue();
        object? key;
        if (!string.IsNullOrWhiteSpace(variableValue?.StringValue))
            key = variableValue.StringValue;
        else if (variableValue?.ObjectValue != null)
            key = variableValue.ObjectValue;
        else if (variableValue?.ListValue != null)
            key = variableValue.ListValue;
        else return result;

        result.StringValue = GetValueAsString(key);
        result.ObjectValue = GetValueAsObject(key);
        result.ListValue = GetValueAsList(key);
        return result;
    }
    /// <summary>
    /// Returns value of variable from given key. Key can contain variables.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="asVariable">Wraps whole expression in double braces to evaluate value.</param>
    /// <returns></returns>
    public string? GetValueAsString(object? key, bool asVariable = false)
    {
        if (key is not string stringKey)
            return null;

        if (string.IsNullOrEmpty(stringKey)) return null;
        if (asVariable)
            stringKey = $"{{{{ {stringKey} }}}}";

        return GetValue(stringKey) as string;
    }

    public VariableValueObject? GetValueAsObject(object? key, bool asVariable = false)
    {
        //If object was passed directly as key, then we need to search all basic properties inside and replace variable tags inside and return it.
        if (key is VariableValueObject objectKey)
        {
            foreach (var item in objectKey.Keys)
                if (objectKey[item]?.ObjectValue != null)
                    objectKey[item] = new VariableValue { ObjectValue = GetValueAsObject(objectKey[item]!.ObjectValue, asVariable) };
                else if (objectKey[item]?.ListValue != null)
                    objectKey[item] = new VariableValue { ListValue = GetValueAsList(objectKey[item]!.ListValue, asVariable) };
                else if (objectKey[item]?.StringValue != null)
                    objectKey[item] = new VariableValue { StringValue = GetValueAsString(objectKey[item]!.StringValue, asVariable) };
                else
                    output.Error("Unknown type");
            return objectKey;
        }

        //If something else than object was passed, we assume it can be string than can evaluate to list.
        if (key is not string stringKey)
            return null;

        if (string.IsNullOrEmpty(stringKey)) return null;
        if (asVariable)
            stringKey = $"{{{{ {stringKey} }}}}";

        return GetValue(stringKey) as VariableValueObject;
    }

    public VariableValueList? GetValueAsList(object? key, bool asVariable = false)
    {
        //If list was passed directly as key, then we need to search all basic properties inside and replace variable tags inside and return it.
        if (key is VariableValueList listKey)
        {
            foreach (var element in listKey)
                //Replace variables with values in all object.
                foreach (var item in element.Keys)
                    if (element[item]?.ObjectValue != null)
                        element[item] = new VariableValue { ObjectValue = GetValueAsObject(element[item]!.ObjectValue, asVariable) };
                    else if (element[item]?.ListValue != null)
                        element[item] = new VariableValue { ListValue = GetValueAsList(element[item]!.ListValue, asVariable) };
                    else if (element[item]?.StringValue != null)
                        element[item] = new VariableValue { StringValue = GetValueAsString(element[item]!.StringValue, asVariable) };
                    else
                        output.Error("Unknown type");
                
            return listKey;
        }

        //If something else than list was passed, we assume it can be string than can evaluate to list.
        if (key is not string stringKey)
            return null;

        if (string.IsNullOrEmpty(stringKey)) return null;
        if (asVariable)
            stringKey = $"{{{{ {stringKey} }}}}";

        return GetValue(stringKey) as VariableValueList;
    }

    /// <summary>
    /// Returns value of variable from given key. Key can contain variables.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="asVariable">Wraps whole expression in double braces to evaluate value.</param>
    /// <returns></returns>
    public int? GetValueAsInt(object? key, bool asVariable = false)
    {
        var stringValue = GetValueAsString(key, asVariable);
        if (string.IsNullOrWhiteSpace(stringValue)) 
            return null;
        if (int.TryParse(stringValue, out var intValue))
            return intValue;

        output.Error($"Failed to convert value {stringValue} to number.");
        return null;
    }

    private object? GetValue(string key)
    {
        // Check if the input is null, empty, or contains only whitespace
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var resolvedKey = key;
        var variableName = ExtractVariableBetweenDelimiters(resolvedKey);
        while (variableName != null)
        {
            var evaluatedValue = EvaluateValue(variableName);
            if (evaluatedValue?.StringValue != null)
            {
                resolvedKey = ReplaceVariableInString(resolvedKey, variableName, evaluatedValue.StringValue);
                variableName = ExtractVariableBetweenDelimiters(resolvedKey);
            }
            else
                return evaluatedValue;
        }
        return resolvedKey;
    }

    /// <summary>
    /// Gets the value of variable by given key. Key cannot contain any variables inside.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private VariableValue? EvaluateValue(string key)
    { 
        var builtIn = GetSubValue(_builtIn, key);
        var local = GetSubValue(_local, key);
        var session = GetSubValue(_session, key);
        var changes = GetSubValue(_changes, key);

        var isList = false;
        if (KeyIsTopLevel(key) &&
            (builtIn?.ListValue != null 
            || local?.ListValue != null
            || session?.ListValue != null
            || changes?.ListValue != null))
            isList = true; //If whole variable value is requested and it is list, values from all scopes will be combined.

        //if (isList)
        //    if (builtIn as Dictionary<string, object?> != null
        //        || local as Dictionary<string, object?> != null
        //        || session as Dictionary<string, object?> != null
        //        || changes as Dictionary<string, object?> != null)
        //        {
        //        output.Error($"Problem while evaluating key {key}: types are not consistant between scopes.");
        //        return null;
        //        }
        
        if (!isList)
            return changes ?? session ?? local ?? builtIn; 
        else
        {
            //Return all rows
            var result = new VariableValueList();
            if (changes?.ListValue != null)
                foreach (var item in changes.ListValue)
                    result.Add(item);

            if (session?.ListValue != null)
                foreach (var item in session.ListValue)
                    if (!result.Any(r => r["key"]?.StringValue?.Equals(item["key"]) == true))
                        result.Add(item);

            if (local?.ListValue != null)
                foreach (var item in local.ListValue)
                    if (!result.Any(r => r["key"]?.StringValue?.Equals(item["key"]) == true))
                        result.Add(item);
            
            if (builtIn?.ListValue != null)
                foreach (var item in builtIn.ListValue)
                    if (!result.Any(r => r["key"]?.StringValue?.Equals(item["key"]) == true))
                        result.Add(item);

            return PackIntoVariableValue(result);
        }
    }

    /// <summary>
    /// Find value of variable by key. It can be nested inside.
    /// </summary>
    /// <param name="variables">List of variables. First element from key</param>
    /// <param name="key">Full key in form variable.property[listIndex].subProperty</param>
    /// <returns></returns>
    private VariableValue? GetSubValue(List<Variable> variables, string key)
    {
        VariableValue? result = null;
        var pattern = @"([a-zA-Z0-9_\-\s]+)|\[(.*?)]";
        var matches = Regex.Matches(key, pattern);


        for (int i = 0; i < matches.Count; i++)
        {
            if (i == 0 && matches[i].Groups[1].Success)
                result = variables.FirstOrDefault(v => v.Key.Equals(matches[i].Groups[1].Value, StringComparison.InvariantCultureIgnoreCase))?.Value;
            else if (i == 0 && matches[i].Groups[2].Success)
                //Key cannot start with index. Abort.
                return null;
            else if (!matches[i].Groups[1].Success && !matches[i].Groups[2].Success)
                //Invalid element in key.
                return null;
            else if (i > 0 && matches[i].Groups[1].Success)
                //Go to subproperty
                result = result?.ObjectValue?[matches[i].Groups[1].Value];
            //else if (i > 0 && matches[i].Groups[2].Success)
            //    //Go to element in list
            //    result = result?.ListValue?.FirstOrDefault(v => v["key"].StringValue?.Equals(matches[i].Groups[2].Value, StringComparison.InvariantCultureIgnoreCase) == true);
            if (result == null)
                return result;
        }

        return result;
    }

    private VariableValue PackIntoVariableValue(object? value) => new VariableValue
    {
        StringValue = value as string,
        ObjectValue = value as VariableValueObject,
        ListValue = value as VariableValueList
    };

    /// <summary>
    /// Extracts the string between specified delimiters within a given input string.
    /// </summary>
    /// <param name="input">The input string from which to extract the variable.</param>
    /// <returns>
    /// The extracted string between the delimiters, or null if the delimiters are not found.
    /// </returns>
    /// <remarks>
    /// This method currently looks for the delimiters "{{" and "}}", but the delimiters may be customized in future versions.
    /// </remarks>
    private string? ExtractVariableBetweenDelimiters(string input)
    {
        // Find the position of the closing braces }}
        int closingIndex = input.IndexOf("}}");
        if (closingIndex == -1) return null;

        // Find the position of the opening braces {{ before the closing braces
        int openingIndex = input.LastIndexOf("{{", closingIndex);
        if (openingIndex == -1) return null;

        // Extract the string between {{ and }}
        return input.Substring(openingIndex + 2, closingIndex - openingIndex - 2).Trim();
    }

    /// <summary>
    /// Replaces all occurrences of a variable placeholder between delimiters with the provided value.
    /// </summary>
    /// <param name="input">The input string that contains placeholders.</param>
    /// <param name="variableName">The name of the variable to replace in the placeholders.</param>
    /// <param name="variableValue">The value to replace the variable placeholder with.</param>
    /// <returns>
    /// The input string with all instances of the variable placeholder replaced with the variable value.
    /// </returns>
    /// <remarks>
    /// This method searches for the pattern "{{ variableName }}" allowing for any amount of whitespace around the variable name.
    /// </remarks>
    private string ReplaceVariableInString(string input, string variableName, string? variableValue)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(variableName))
            return input;

        // Pattern to match {{ variableName }} with possible whitespaces
        string pattern = $@"\{{\{{\s*{Regex.Escape(variableName)}\s*\}}\}}";

        // Replace all occurrences with the variable value
        return Regex.Replace(input, pattern, variableValue ?? string.Empty);
    }

    /// <summary>
    /// Indicates if key point to top level of variable value (whole variable value).
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private bool KeyIsTopLevel(string key) => !key.Contains(".") && !key.Contains("[");

    /// <summary>
    /// Returns variable name part of key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string GetTopLevel(string key) => key.Split(['.', '['], StringSplitOptions.RemoveEmptyEntries).First();

    public void SetVariableValue(VariableScope scope, string variableName, object? value, string? description = null)
    {
        var topLevel = GetTopLevel(variableName);
        var existingVariable = _changes.FirstOrDefault(v =>
            v.Key.Equals(topLevel, StringComparison.InvariantCultureIgnoreCase) && v.Scope == scope);

        if (variableName != topLevel && existingVariable == null) //new element in list
        {
            if (value is VariableValueObject objectValue)
                _changes.Add(new Variable { Key = topLevel, Value = new VariableValue { ListValue = new VariableValueList { objectValue } }, Scope = scope, Description = description });
            else if (value is VariableValueList listValue)
                _changes.Add(new Variable { Key = topLevel, Value = new VariableValue { ListValue = listValue }, Scope = scope, Description = description });
            else
                output.Error("Tried to insert wrong type into list");
        }
        else if (variableName == topLevel && existingVariable == null) //new variable
            _changes.Add(new Variable { Key = variableName, Value = new VariableValue { StringValue = (string) value }, Scope = scope, Description = description });
        else if (existingVariable != null) //existing variable
            existingVariable.Value = new VariableValue { StringValue = (string)value };
    }
}