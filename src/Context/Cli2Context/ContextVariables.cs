using Models;
using Models.Interfaces.Context;
using System.Collections.Immutable;

namespace Cli2Context;

public class ContextVariables : IContextVariables
{
    private readonly ContextVariableStorage _variableStorage = new();
    private readonly ContextVariableResolver _variableResolver;

    public ContextVariables(IOutput output) => _variableResolver = new ContextVariableResolver(output, _variableStorage);

    public string CurrentSessionName
    {
        get => _variableResolver.ResolveVariableValue(new VariableValue("currentSessionName"), true)?.TextValue ?? "session1";
        set => SetVariableValue(VariableScope.Application, "currentSessionName", value);
    }

    public string? CurrentlyProcessedElement
    {
        get => _variableResolver.ResolveVariableValue(new VariableValue("currentlyProcessedElement"), true)?.TextValue;
        set => SetVariableValue(VariableScope.Command, "currentlyProcessedElement", value);
    }

    public VariableValue? ResolveVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable = false)
    => _variableResolver.ResolveVariableValue(variableValue, treatTextValueAsVariable);

    public void SetVariableList(RepositoryLocation repositoryLocation, List<Variable?> elementsWithContent)
    {
        var elementsToImport = elementsWithContent.Where(v => v != null).Cast<Variable>();

        switch (repositoryLocation)
        {
            case RepositoryLocation.BuiltIn:
                _variableStorage.BuiltIn = elementsToImport.Select(element => element with { Scope = VariableScope.Application }).ToImmutableList();
                break;
            case RepositoryLocation.Local:
                _variableStorage.Local = elementsToImport.Select(element => element with { Scope = VariableScope.Application }).ToImmutableList();
                break;
            case RepositoryLocation.Session:
                _variableStorage.Session = elementsToImport.Select(element => element with { Scope = VariableScope.Session }).ToImmutableList();
                break;
            default:
                throw new InvalidOperationException($"Unknown repository location: {repositoryLocation}");
        }
    }

    public void SetVariableValue(VariableScope scope, string variableName, object? value, string? description = null)
    {
        // var topLevel = GetTopLevel(variableName);
        // var existingVariable = _changes.FirstOrDefault(v =>
        //     v.Key.Equals(topLevel, StringComparison.InvariantCultureIgnoreCase) && v.Scope == scope);

        // if (variableName != topLevel && existingVariable == null) // new element in list
        // {
        //     if (value is VariableValueObject objectValue)
        //         _changes.Add(new Variable { Key = topLevel, Value = new VariableValue { ListValue = new VariableValueList { objectValue } }, Scope = scope, Description = description });
        //     else if (value is VariableValueList listValue)
        //         _changes.Add(new Variable { Key = topLevel, Value = new VariableValue { ListValue = listValue }, Scope = scope, Description = description });
        //     else
        //         output.Error("Tried to insert wrong type into list");
        // }
        // else if (variableName == topLevel && existingVariable == null) // new variable
        //     _changes.Add(new Variable { Key = variableName, Value = new VariableValue { TextValue = (string?)value }, Scope = scope, Description = description });
        // else if (existingVariable != null) // existing variable
        //     existingVariable.Value = new VariableValue { TextValue = (string?)value };
    }

    /// <inheritdoc />
    // public VariableValue? ResolveVariableValue(VariableValue? variableValue, bool treatTextValueAsVariable = false)
    // {
    //     if (variableValue == null)
    //         return null;
    //     var result = variableValue with { };
    //     if (result.TextValue != null)
    //         result = ResolveTextKey(variableValue.TextValue, treatTextValueAsVariable) ?? variableValue;
    //     if (result?.ObjectValue != null)
    //         result = ResolveObjectKey(result.ObjectValue);
    //     if (result?.ListValue != null)
    //         result = ResolveListKey(result.ListValue);

    //     return result;
    // }

    /// <summary>
    /// Indicates if key points to top level of variable value (whole variable value).
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    // private bool KeyIsTopLevel(string key) => !key.Contains(".") && !key.Contains("[");

    /// <summary>
    /// Returns variable name part of key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    // private string GetTopLevel(string key) => key.Split(new[] { '.', '[' }, StringSplitOptions.RemoveEmptyEntries).First();
}
