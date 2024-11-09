namespace Cli2Context;

internal class ContextVariableWriter(IContext context, ContextVariableStorage variableStorage)
{
    public void WriteVariableValue(VariableScope scope, string path, DynamicValue value, string? locationId)
    {

        //Replacing whole variable.
        if (VariableValuePath.PathIsTopLevel(path))
            WriteVariableValueOnTopLevelVariable(scope, path, value, locationId);
        //Adding or replacing element in list.
        else if (VariableValuePath.PathIsTopLevelList(path))
            WriteVariableValueOnTopLevelList(scope, path, value, locationId);
        else
            context.Terminate("Writing to sub-property is not supported");
    }

    internal void WriteVariableValueOnTopLevelVariable(VariableScope scope, string path, DynamicValue value, string? locationId)
    {
        //Find current values.
        var existingVariable =
            variableStorage.Command.FirstOrDefault(v => v.Key == path)
            ?? variableStorage.Session.FirstOrDefault(v => v.Key == path)
            ?? variableStorage.Application.FirstOrDefault(v => v.Key == path)
            ?? variableStorage.BuiltIn.FirstOrDefault(v => v.Key == path);
        var resolvedLocationId = existingVariable?.LocationId ?? locationId;

        //Remove earlier changes.
        variableStorage.Command.RemoveAll(v => v.Key == path);
        if (scope == VariableScope.Session) variableStorage.Session.RemoveAll(v => v.Key == path);
        if (scope == VariableScope.Application) variableStorage.Application.RemoveAll(v => v.Key == path);

        var variableToInsert = new Variable { Key = path, Value = value, LocationId = resolvedLocationId };
        if (scope == VariableScope.Command) variableStorage.Command.Add(variableToInsert);
        if (scope == VariableScope.Session) variableStorage.Session.Add(variableToInsert);
        if (scope == VariableScope.Application) variableStorage.Application.Add(variableToInsert);
    }

    internal void WriteVariableValueOnTopLevelList(VariableScope scope, string path, DynamicValue value, string? locationId)
    {
        var variableName = VariableValuePath.GetVariableName(path);
        var key = VariableValuePath.TopLevelListKey(path);
        if (key == null)
        {
            context.Terminate("Error while updating variable value.");
            return;
        }
        if (value.ObjectValue == null)
        {
            context.Terminate("List can contain only objects.");
            return;
        }
        if (value.ObjectValue["key"]?.TextValue != key)
        {
            context.Terminate("Object key must have same value as index of updated list.");
            return;
        }

        Variable? existingChange = null;
        //Remove earlier changes of given key in list.
        if (scope == VariableScope.Command)
            //When setting value for command scope, previous command scoped values are removed.
            existingChange = variableStorage.Command.FirstOrDefault(v => v.Key == variableName);
        else if (scope == VariableScope.Session)
        {
            //When setting value for session scope, previous command scoped values are removed.
            variableStorage.Command.RemoveAll(v => v.Key == variableName);
            existingChange = variableStorage.Session.FirstOrDefault(v => v.Key == variableName);
        }
        else
        {
            //When setting value for application scope, previous command and session scoped values are removed.
            variableStorage.Command.RemoveAll(v => v.Key == variableName);
            variableStorage.Session.RemoveAll(v => v.Key == variableName);
            existingChange = variableStorage.Application.FirstOrDefault(v => v.Key == variableName);
        }

        if (existingChange != null)
        {
            //When given list element was already edited - remove previous value and add new one.
            var newList = (existingChange.Value.ListValue?.RemoveAll(v => v["key"].TextValue == key) ?? new DynamicValueList()).Add(value.ObjectValue);
            existingChange.Value = new DynamicValue(newList);
                    
            return;
        }

        //Find current values.
        var existingVariable =
            variableStorage.Command.FirstOrDefault(v => v.Key == variableName)
            ?? variableStorage.Session.FirstOrDefault(v => v.Key == variableName)
            ?? variableStorage.Application.FirstOrDefault(v => v.Key == variableName)
            ?? variableStorage.BuiltIn.FirstOrDefault(v => v.Key == variableName);
        var resolvedLocationId = existingVariable?.LocationId ?? locationId;

        //When given list element was not yet edited.
        var newVariable = new Variable { Key = variableName, Value = new DynamicValue(new DynamicValueList([value.ObjectValue])), LocationId = resolvedLocationId };
        if (scope == VariableScope.Command) variableStorage.Command.Add(newVariable);
        if (scope == VariableScope.Session) variableStorage.Session.Add(newVariable);
        if (scope == VariableScope.Application) variableStorage.Application.Add(newVariable);
    }
}