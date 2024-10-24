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
        get => GetValueAsText("currentSessionName", true) ?? "session1";
        set => SetVariableValue(VariableScope.Application, "currentSessionName", value);
    }

    public string? CurrentlyProcessedElement
    {
        get => GetValueAsText("currentlyProcessedElement", true);
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

    /// <inheritdoc />
    public VariableValue? ResolveVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable = false)
    {
        if (variableValue == null)
            return null;
        var result = variableValue with { };
        if (result.TextValue != null && !treatTextValueAsVariable)
            result = ResolveTextKey(variableValue.TextValue) ?? variableValue;
        if (result.TextValue != null && treatTextValueAsVariable)
            result = ResolveTextKey($"{{{{ {variableValue.TextValue} }}}}");
        if (result?.ObjectValue != null)
            foreach (var item in result.ObjectValue.Keys)
                result = result with { ObjectValue = result.ObjectValue.With(item, ResolveVariableValue(result.ObjectValue[item])) };
        if (result?.ListValue != null)
            foreach (var element in result.ListValue.Items)
                foreach (var item in element.Keys)
                    result = result with { ObjectValue = element.With(item, ResolveVariableValue(element[item])) };

        return result;
    }

    /// <summary>
    /// Try to resolve variable value into Text type. Only text key is accepted.
    /// </summary>
    /// <param name="key">Text</param>
    /// <param name="treatTextValueAsVariable">Whole TextValue is treated as variable.</param>
    /// <returns></returns>
    private string? GetValueAsText(object? key, bool treatTextValueAsVariable = false)
    {
        if (key is not string stringKey)
            return null;

        if (string.IsNullOrEmpty(stringKey)) return null;
        if (treatTextValueAsVariable)
            stringKey = $"{{{{ {stringKey} }}}}";

        return ResolveTextKey(stringKey)?.TextValue;
    }

    /// <summary>
    /// Try to resolve variable value into Object type.
    /// </summary>
    /// <param name="key">Text or object.</param>
    /// <returns></returns>
    public VariableValueObject? GetValueAsObject(object? key)
    {
        //If text was passed as key, we assume it can resolve to object.
        if (key is string stringKey && !string.IsNullOrEmpty(stringKey))
            key = ResolveTextKey(stringKey)?.ObjectValue;

        //If key now contains object, then we need to search all basic properties inside and replace variable tags inside and return it.
        if (key is not VariableValueObject objectKey)
            return null;
        
        //foreach (var item in objectKey.Keys)
        //    if (objectKey[item]?.ObjectValue != null)
        //        objectKey[item] = new VariableValue { ObjectValue = GetValueAsObject(objectKey[item]!.ObjectValue) };
        //    else if (objectKey[item]?.ListValue != null)
        //        objectKey[item] = new VariableValue { ListValue = GetValueAsList(objectKey[item]!.ListValue) };
        //    else if (objectKey[item]?.TextValue != null)
        //        objectKey[item] = new VariableValue { TextValue = GetValueAsText(objectKey[item]!.TextValue) };
        //    else
        //        output.Error("Unknown type");
        return objectKey;
    }


    /// <summary>
    /// Try to resolve variable value into List type. 
    /// </summary>
    /// <param name="key">Text or list.</param>
    /// <returns></returns>
    public VariableValueList? GetValueAsList(object? key)
    {
        //If text was passed as key, we assume it can resolve to list.
        //if (key is string stringKey && !string.IsNullOrEmpty(stringKey))
        //    key = GetValueWithNoTypeFromTextKey(stringKey)?.ListValue;

        //If key now contains list, then we need to search all basic properties inside and replace variable tags inside and return it.
        if (key is not VariableValueList listKey)
            return null;
        
        //foreach (var element in listKey)
        //    foreach (var item in element.Keys)
        //        if (element[item]?.ObjectValue != null)
        //            element[item] = new VariableValue { ObjectValue = GetValueAsObject(element[item]!.ObjectValue) };
        //        else if (element[item]?.ListValue != null)
        //            element[item] = new VariableValue { ListValue = GetValueAsList(element[item]!.ListValue) };
        //        else if (element[item]?.TextValue != null)
        //            element[item] = new VariableValue { TextValue = GetValueAsText(element[item]!.TextValue) };
        //        else
        //            output.Error("Unknown type");
                
        return listKey;
    }

    /// <summary>
    /// Resolves text key. 
    /// </summary>
    /// <param name="key">Text value that might contain variable tags.</param>
    /// <returns>Resolved text, object or list.</returns>
    private VariableValue? ResolveTextKey(string? key)
    {
        // Check if the input is null, empty, or contains only whitespace
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var resolvedKey = key;
        var variableName = ExtractVariableBetweenDelimiters(resolvedKey);
        VariableValue? resolvedValue = null;
        while (variableName != null)
        {
            resolvedValue = ResolveSingleValue(variableName);
            
            if (resolvedValue?.TextValue == null)
                return resolvedValue;
            
            resolvedKey = ReplaceVariableInString(resolvedKey, variableName, resolvedValue.TextValue);
            variableName = ExtractVariableBetweenDelimiters(resolvedKey);
            
        }
        return resolvedValue;
    }

    /// <summary>
    /// Gets the value of variable by given key. Key cannot contain any variables inside.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private VariableValue? ResolveSingleValue(string key)
    { 
        var builtIn = GetSubValue(_builtIn, key);
        var local = GetSubValue(_local, key);
        var session = GetSubValue(_session, key);
        var changes = GetSubValue(_changes, key);

        //var isList = false;
        //if (KeyIsTopLevel(key) &&
        //    (builtIn?.ListValue != null 
        //    || local?.ListValue != null
        //    || session?.ListValue != null
        //    || changes?.ListValue != null))
        //    isList = true; //If whole variable value is requested and it is list, values from all scopes will be combined.
       
        //if (!isList)
            return changes ?? session ?? local ?? builtIn; 
        //else
        //{
        //    //Return all rows
        //    var result = new VariableValueList();
        //    if (changes?.ListValue != null)
        //        foreach (var item in changes.ListValue)
        //            result.Add(item);

        //    if (session?.ListValue != null)
        //        foreach (var item in session.ListValue)
        //            if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
        //                result.Add(item);

        //    if (local?.ListValue != null)
        //        foreach (var item in local.ListValue)
        //            if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
        //                result.Add(item);
            
        //    if (builtIn?.ListValue != null)
        //        foreach (var item in builtIn.ListValue)
        //            if (!result.Any(r => r["key"]?.TextValue?.Equals(item["key"]) == true))
        //                result.Add(item);

        //    return new VariableValue { ListValue = result };
        //}
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
        //var topLevel = GetTopLevel(variableName);
        //var existingVariable = _changes.FirstOrDefault(v =>
        //    v.Key.Equals(topLevel, StringComparison.InvariantCultureIgnoreCase) && v.Scope == scope);

        //if (variableName != topLevel && existingVariable == null) //new element in list
        //{
        //    if (value is VariableValueObject objectValue)
        //        _changes.Add(new Variable { Key = topLevel, Value = new VariableValue { ListValue = new VariableValueList { objectValue } }, Scope = scope, Description = description });
        //    else if (value is VariableValueList listValue)
        //        _changes.Add(new Variable { Key = topLevel, Value = new VariableValue { ListValue = listValue }, Scope = scope, Description = description });
        //    else
        //        output.Error("Tried to insert wrong type into list");
        //}
        //else if (variableName == topLevel && existingVariable == null) //new variable
        //    //Add other types of new value
        //    _changes.Add(new Variable { Key = variableName, Value = new VariableValue { TextValue = (string) value }, Scope = scope, Description = description });
        //else if (existingVariable != null) //existing variable
        //    existingVariable.Value = new VariableValue { TextValue = (string)value };
    }
}