public interface IWriter
{
    void WriteVariableValue(VariableScope scope, string? sessionName, string path, DynamicValue value, string? repositoryElementId);
}

public class Writer(IFlow flow, Storage variableStorage, IRepository repository) : IWriter
{
    public void WriteVariableValue(VariableScope scope, string? sessionName, string path, DynamicValue value, string? repositoryElementId)
    {
        var fullRepositoryElementId = GetFullRepositoryElementId(scope, sessionName, repositoryElementId);

        //Replacing whole variable.
        if (ValuePath.PathIsTopLevel(path))
            WriteVariableValueOnTopLevelVariable(scope, path, value, fullRepositoryElementId);
        //Adding or replacing element in list.
        else if (ValuePath.PathIsTopLevelList(path))
            WriteVariableValueOnTopLevelList(scope, path, value, fullRepositoryElementId);
        else
            flow.Terminate("Writing to sub-property is not supported");
    }

    /// <summary>
    /// If SetVariable operation contains id parameter, it points just to file name. Here we add full path.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="sessionName"></param>
    /// <param name="repositoryElementId"></param>
    /// <returns></returns>
    private string? GetFullRepositoryElementId(VariableScope scope, string? sessionName, string? repositoryElementId)
    {
        var resolvedRepositoryElementId = repositoryElementId;
        if (resolvedRepositoryElementId != null && scope != VariableScope.Command)
        {
            var basePath = repository.GetPath(scope == VariableScope.Session ? RepositoryLocation.Session : RepositoryLocation.Application, sessionName);
            resolvedRepositoryElementId = Path.Combine(basePath, resolvedRepositoryElementId);
        }

        return resolvedRepositoryElementId;
    }

    internal void WriteVariableValueOnTopLevelVariable(VariableScope scope, string path, DynamicValue value, string? repositoryElementId)
    {
        //Find current values.
        var existingVariable =
            variableStorage.Command.FirstOrDefault(v => v.Key == path)
            ?? variableStorage.Session.FirstOrDefault(v => v.Key == path)
            ?? variableStorage.Application.FirstOrDefault(v => v.Key == path)
            ?? variableStorage.BuiltIn.FirstOrDefault(v => v.Key == path);
        var resolvedRepositoryElementId = existingVariable?.RepositoryElementId ?? repositoryElementId;

        //Remove earlier changes.
        variableStorage.Command.RemoveAll(v => v.Key == path);
        if (scope == VariableScope.Session) variableStorage.Session.RemoveAll(v => v.Key == path);
        if (scope == VariableScope.Application) variableStorage.Application.RemoveAll(v => v.Key == path);

        var variableToInsert = new Variable { Key = path, Value = value, RepositoryElementId = resolvedRepositoryElementId };
        if (scope == VariableScope.Command) variableStorage.Command.Add(variableToInsert);
        if (scope == VariableScope.Session) variableStorage.Session.Add(variableToInsert);
        if (scope == VariableScope.Application) variableStorage.Application.Add(variableToInsert);
    }

    internal void WriteVariableValueOnTopLevelList(VariableScope scope, string path, DynamicValue value, string? repositoryElementId)
    {
        var variableName = ValuePath.GetVariableName(path);
        var key = ValuePath.TopLevelListKey(path);
        if (key == null)
        {
            flow.Terminate("Error while updating variable value.");
            return;
        }
        if (value.ObjectValue == null)
        {
            flow.Terminate("List can contain only objects.");
            return;
        }
        if (value.ObjectValue["key"]?.TextValue != key)
        {
            flow.Terminate("Object key must have same value as index of updated list.");
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
        var resolvedRepositoryElementId = repositoryElementId ?? existingVariable?.RepositoryElementId;

        //When given list element was not yet edited.
        var newVariable = new Variable { Key = variableName, Value = new DynamicValue(new DynamicValueList([value.ObjectValue])), RepositoryElementId = resolvedRepositoryElementId };
        if (scope == VariableScope.Command) variableStorage.Command.Add(newVariable);
        if (scope == VariableScope.Session) variableStorage.Session.Add(newVariable);
        if (scope == VariableScope.Application) variableStorage.Application.Add(newVariable);
    }
}