using Models;
using Models.Interfaces.Context;

namespace Cli2Context;

internal class ContextVariableWriter(IContext context, ContextVariableStorage variableStorage)
{
    public void SetVariableValue(VariableScope scope, string path, VariableValue value, string? description = null)
    {

        //Replacing whole variable.
        if (VariableValuePath.PathIsTopLevel(path))
            SetVariableValueOnTopLevelVariable(scope, path, value, description);
        //Adding or replacing element in list.
        else if (VariableValuePath.PathIsTopLevelList(path))
            SetVariableValueOnTopLevelList(scope, path, value);
        else
        {
            context.Services.Output.Error("Writing to sub-property is not supported");

        }
    }

    internal void SetVariableValueOnTopLevelVariable(VariableScope scope, string path, VariableValue value, string? description)
    {
        //Find current values.
        var existingVariable =
            variableStorage.Changes.FirstOrDefault(v => v.Key == path && v.Scope == VariableScope.Command)
            ?? variableStorage.Changes.FirstOrDefault(v => v.Key == path && v.Scope == VariableScope.Session)
            ?? variableStorage.Session.FirstOrDefault(v => v.Key == path && v.Scope == VariableScope.Session)
            ?? variableStorage.Changes.FirstOrDefault(v => v.Key == path && v.Scope == VariableScope.Application)
            ?? variableStorage.Local.FirstOrDefault(v => v.Key == path && v.Scope == VariableScope.Application)
            ?? variableStorage.BuiltIn.FirstOrDefault(v => v.Key == path && v.Scope == VariableScope.Application);
        var resolvedDescription = description ?? existingVariable?.Description;
        var allowedValues = existingVariable?.AllowedValues ?? new List<string>();
        var locationId = existingVariable?.LocationId;

        //Remove earlier changes.
        if (scope == VariableScope.Command)
            //When setting value for command scope, only previous command scoped values are removed.
            variableStorage.Changes.RemoveAll(v => v.Key == path && v.Scope == VariableScope.Command);
        else if (scope == VariableScope.Session)
            //When setting value for session scope, only application level changes remain.
            variableStorage.Changes.RemoveAll(v => v.Key == path && v.Scope is VariableScope.Command or VariableScope.Session);
        else if (scope == VariableScope.Application)
            //When setting value for application scope, all previous changes are removed.
            variableStorage.Changes.RemoveAll(v => v.Key == path);

        variableStorage.Changes.Add(new Variable { Key = path, Value = value, Description = resolvedDescription, Scope = scope, LocationId = locationId, AllowedValues = allowedValues });
    }

    internal void SetVariableValueOnTopLevelList(VariableScope scope, string path, VariableValue value)
    {
        var variableName = VariableValuePath.GetVariableName(path);
        var key = VariableValuePath.TopLevelListKey(path);
        if (key == null)
        {
            context.Services.Output.Error("Error while updating variable value.");
            return;
        }
        if (value.ObjectValue == null)
        {
            context.Services.Output.Error("List can contain only objects.");
            return;
        }
        if (value.ObjectValue["key"]?.TextValue != key)
        {
            context.Services.Output.Error("Object key must have same value as index of updated list.");
            return;
        }

        Variable? existingChange = null;
        //Remove earlier changes of given key in list.
        if (scope == VariableScope.Command)
            //When setting value for command scope, previous command scoped values are removed.
            existingChange = variableStorage.Changes.FirstOrDefault(v => v.Key == variableName && v.Scope == VariableScope.Command);
        else if (scope == VariableScope.Session)
        {
            //When setting value for session scope, previous command scoped values are removed.
            variableStorage.Changes.RemoveAll(v => v.Key == variableName && v.Scope == VariableScope.Command);
            existingChange = variableStorage.Changes.FirstOrDefault(v => v.Key == variableName && v.Scope == VariableScope.Session);
        }
        else
        {
            //When setting value for application scope, previous command and session scoped values are removed.
            variableStorage.Changes.RemoveAll(v => v.Key == variableName && v.Scope is VariableScope.Command or VariableScope.Session);
            existingChange = variableStorage.Changes.FirstOrDefault(v => v.Key == variableName && v.Scope == VariableScope.Application);
        }


        if (existingChange != null)
        {
            //When given list element was already edited - remove previous value and add new one.
            existingChange.Value = existingChange.Value with
            {
                ListValue = (existingChange.Value.ListValue?.RemoveAll(v => v["key"].TextValue == key) ?? new VariableValueList())
                    .Add(value.ObjectValue),
                ObjectValue = null,
                TextValue = null,
            };
        }
        else
            //When given list element was not yet edited.
            variableStorage.Changes.Add(new Variable { Scope = scope, Key = variableName, Value = new VariableValue(new VariableValueList([value.ObjectValue])) });

    }

}
