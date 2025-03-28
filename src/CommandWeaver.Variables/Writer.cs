/// <summary>
/// Provides methods to write variable values to a repository or in-memory storage.
/// </summary>
public interface IWriter
{
    /// <summary>
    /// Writes a variable value to a specific scope and repository location.
    /// </summary>
    /// <param name="scope">The scope of the variable (e.g., Command, Session, Application).</param>
    /// <param name="sessionName">The session name if the scope is Session.</param>
    /// <param name="path">The path of the variable to write.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="repositoryElementId">The optional repository element ID where the value is stored.</param>
    void WriteVariableValue(VariableScope scope, string? sessionName, string path, DynamicValue value, string? repositoryElementId);
}

/// <inheritdoc />
public class Writer(IVariableStorage variableStorage, IRepository repository) : IWriter
{
    /// <inheritdoc />
    public void WriteVariableValue(VariableScope scope, string? sessionName, string path, DynamicValue value, string? repositoryElementId)
    {
        var fullRepositoryElementId = GetFullRepositoryElementId(scope, sessionName, repositoryElementId);

        if (ValuePath.PathIsTopLevel(path))
            WriteVariableValueOnTopLevelVariable(scope, path, value, fullRepositoryElementId);
        else if (ValuePath.PathIsTopLevelList(path))
            WriteVariableValueOnTopLevelList(scope, path, value, fullRepositoryElementId);
        else
            throw new CommandWeaverException("Writing to sub-property is not supported.");
    }

    private string? GetFullRepositoryElementId(VariableScope scope, string? sessionName, string? repositoryElementId)
    {
        if (repositoryElementId == null || scope == VariableScope.Command)
            return repositoryElementId;

        var basePath = repository.GetPath(
            scope == VariableScope.Session ? RepositoryLocation.Session : RepositoryLocation.Application,
            sessionName
        );
        return Path.Combine(basePath, repositoryElementId);
    }

    private void WriteVariableValueOnTopLevelVariable(VariableScope scope, string path, DynamicValue value, string? repositoryElementId)
    {
        variableStorage.RemoveAllInScope(VariableScope.Command, v => v.Key == path);
        if (scope is VariableScope.Session or VariableScope.Application) variableStorage.RemoveAllInScope(VariableScope.Session, v => v.Key == path);
        if (scope == VariableScope.Application) variableStorage.RemoveAllInScope(VariableScope.Application, v => v.Key == path);

        var resolvedRepositoryElementId = ResolveRepositoryElementId(repositoryElementId, path);
        if (scope != VariableScope.Command && string.IsNullOrWhiteSpace(resolvedRepositoryElementId))
            throw new CommandWeaverException("Repository element ID must be specified for non-command scopes.");

        var variableToInsert = new Variable { Key = path, Value = value, RepositoryElementId = resolvedRepositoryElementId };
        variableStorage.Add(scope, variableToInsert);
    }

    private void WriteVariableValueOnTopLevelList(VariableScope scope, string path, DynamicValue value, string? repositoryElementId)
    {
        var variableName = ValuePath.GetVariableName(path);
        var key = ValuePath.TopLevelListKey(path);
        if (key == null)
            throw new CommandWeaverException("Error while updating variable value.");

        var existingChange = variableStorage.FirstOrDefault(scope, v => v.Key == variableName);
        variableStorage.RemoveAllBelowScope(scope, v => v.Key == variableName);
        
        if (existingChange != null)
        {
            var newList = (existingChange.Value.ListValue?.RemoveAll(v => v.ObjectValue?["key"].TextValue == key) ?? new DynamicValueList()).Add(value);
            existingChange.Value = new DynamicValue(newList);
            return;
        }

        var resolvedRepositoryElementId = ResolveRepositoryElementId(repositoryElementId, variableName);

        var variableToInsert = new Variable { Key = variableName, Value = new DynamicValue(new DynamicValueList([value])), RepositoryElementId = resolvedRepositoryElementId };
        variableStorage.Add(scope, variableToInsert);
    }
    
    private string? ResolveRepositoryElementId(string? repositoryElementId, string key)
        => repositoryElementId ?? variableStorage.FirstOrDefault(v => v.Key == key)?.RepositoryElementId;
}
