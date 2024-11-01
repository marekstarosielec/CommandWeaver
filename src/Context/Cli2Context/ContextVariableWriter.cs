using Models;
using Models.Interfaces.Context;

namespace Cli2Context;

internal class ContextVariableWriter(IOutput output, ContextVariableStorage variableStorage)
{
    public void SetVariableValue(VariableScope scope, string path, VariableValue value, string? description = null)
    {
        //Find current values.
        var existingVariable = GetVariable(path);
        var resolvedDescription = description ?? existingVariable?.Description;
        var allowedValues = existingVariable?.AllowedValues ?? new List<string>();
        var locationId = existingVariable?.LocationId;

        //Replacing whole variable.
        if (VariableValuePath.PathIsTopLevel(path))
            SetVariableValueOnTopLevelVariable(scope, path, value, resolvedDescription, allowedValues, locationId);

        else if (VariableValuePath.PathIsTopLevelList(path))
        {
            
        }
        else
        {

        }
    }

    internal void SetVariableValueOnTopLevelVariable(VariableScope scope, string path, VariableValue value, string? resolvedDescription, List<string> allowedValues, string? locationId)
    {
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

    internal void SetVariableValueOnTopLevelList(VariableScope scope, string path, VariableValue value, string? resolvedDescription, List<string> allowedValues, string? locationId)
    {
        var variableName = VariableValuePath.GetVariableName(path);
        var key = VariableValuePath.TopLevelListKey(path);
        if (key == null)
        {
            output.Error("Error while updating variable value.");
            return;
        }
        if (value.ObjectValue == null)
        {
            output.Error("List can contain only objects.");
            return;
        }
        if (value.ObjectValue["key"]?.TextValue != key)
        {
            output.Error("Object key must have same value as index of updated list.");
            return;
        }

        //Remove earlier changes of given key in list.
        if (scope == VariableScope.Command)
        {
            //When setting value for command scope, only previous command scoped values are removed.
            var existingChange = variableStorage.Changes.FirstOrDefault(v => v.Key == variableName && v.Scope == VariableScope.Command);
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
                if (existingChange.Description != resolvedDescription || existingChange.AllowedValues != allowedValues) 
                    existingChange = existingChange with { Description = resolvedDescription, AllowedValues = allowedValues };
            }
            else
                //When given list element was not yet edited.
                variableStorage.Changes.Add(new Variable { Key = variableName, Value = new VariableValue(new VariableValueList([value.ObjectValue])), Description = resolvedDescription, AllowedValues = allowedValues });

        }
        else if (scope == VariableScope.Session)
        {
            ////When setting value for session scope, only application level changes remain.
            //var existingChange = variableStorage.Changes.FirstOrDefault(v => v.Key == variableName && scope == VariableScope.Command);
            //if (existingChange != null && existingChange.Value?.ListValue?.Count() == 1 && existingChange.Value?.ListValue?.Any(v => v["key"]?.TextValue == key) == true)
            //    //There is already a command scoped change and it contains only change for given index, so we can delete it fully.
            //    variableStorage.Changes.RemoveAll(v => v.Key == variableName && scope == VariableScope.Command);
            //else if (existingChange != null && existingChange.Value?.ListValue?.Count() > 1)
            //    //There is already a command scoped change but it contains more changes, so we can delete only change for given index.
            //    existingChange.Value = existingChange.Value with { ListValue = existingChange.Value.ListValue.RemoveAll(v => v["key"].TextValue == key) };

            //existingChange = variableStorage.Changes.FirstOrDefault(v => v.Key == variableName && scope == VariableScope.Session);
            //if (existingChange != null && existingChange.Value?.ListValue?.Any(v => v["key"]?.TextValue == key) == true)
            //{
            //    //There is already a session scoped change and it contains change for given index, so we can modify it.
            //    existingChange.Value
            //}
            //else if (existingChange != null && existingChange.Value?.ListValue?.Count() > 1)
            //    //There is already a command scoped change but it contains more changes, so we can delete only change for given index.
            //    existingChange.Value.ListValue = existingChange.Value.ListValue.RemoveAll(v => v["key"].TextValue == key);
            //else if (existingChange == null)
            //{

            //}
        }
        //else if (scope == VariableScope.Application)
        //    //When setting value for application scope, all previous changes are removed.
        //    foreach (var item in variableStorage.Changes.Where(v => v.Key == variableName))
        //        item.Value?.ListValue?.RemoveAll(v => v["key"].TextValue == key);

    }

    private Variable? GetVariable(string variableName)
        => variableStorage.Changes.FirstOrDefault(v => v.Key == variableName && v.Scope == VariableScope.Command)
        ?? variableStorage.Changes.FirstOrDefault(v => v.Key == variableName && v.Scope == VariableScope.Session)
        ?? variableStorage.Session.FirstOrDefault(v => v.Key == variableName && v.Scope == VariableScope.Session)
        ?? variableStorage.Changes.FirstOrDefault(v => v.Key == variableName && v.Scope == VariableScope.Application)
        ?? variableStorage.Local.FirstOrDefault(v => v.Key == variableName && v.Scope == VariableScope.Application)
        ?? variableStorage.BuiltIn.FirstOrDefault(v => v.Key == variableName && v.Scope == VariableScope.Application);
}
