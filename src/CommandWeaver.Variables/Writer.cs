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
public class Writer(IFlowService flowService, IVariableStorage variableStorage, IRepository repository) : IWriter
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
        {
            flowService.Terminate("Writing to sub-property is not supported.");
            throw new InvalidOperationException("Unsupported operation: Writing to sub-property is not allowed.");
        }
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
        var existingVariable = variableStorage.Command.FirstOrDefault(v => v.Key == path)
                               ?? variableStorage.Session.FirstOrDefault(v => v.Key == path)
                               ?? variableStorage.Application.FirstOrDefault(v => v.Key == path)
                               ?? variableStorage.BuiltIn.FirstOrDefault(v => v.Key == path);

        var resolvedRepositoryElementId = existingVariable?.RepositoryElementId ?? repositoryElementId;

        variableStorage.Command.RemoveAll(v => v.Key == path);
        if (scope == VariableScope.Session) variableStorage.Session.RemoveAll(v => v.Key == path);
        if (scope == VariableScope.Application) variableStorage.Application.RemoveAll(v => v.Key == path);

        if (scope != VariableScope.Command && string.IsNullOrWhiteSpace(resolvedRepositoryElementId))
        {
            flowService.Terminate("Repository element ID must be specified for non-command scopes.");
            throw new InvalidOperationException("Repository element ID is missing for non-command scope.");
        }

        var variableToInsert = new Variable { Key = path, Value = value, RepositoryElementId = resolvedRepositoryElementId };
        if (scope == VariableScope.Command) variableStorage.Command.Add(variableToInsert);
        if (scope == VariableScope.Session) variableStorage.Session.Add(variableToInsert);
        if (scope == VariableScope.Application) variableStorage.Application.Add(variableToInsert);
    }

    private void WriteVariableValueOnTopLevelList(VariableScope scope, string path, DynamicValue value, string? repositoryElementId)
    {
        var variableName = ValuePath.GetVariableName(path);
        var key = ValuePath.TopLevelListKey(path);
        if (key == null)
        {
            flowService.Terminate("Error while updating variable value.");
            throw new InvalidOperationException("Invalid list key.");
        }

        Variable? existingChange;
        if (scope == VariableScope.Command)
            existingChange = variableStorage.Command.FirstOrDefault(v => v.Key == variableName);
        else if (scope == VariableScope.Session)
        {
            variableStorage.Command.RemoveAll(v => v.Key == variableName);
            existingChange = variableStorage.Session.FirstOrDefault(v => v.Key == variableName);
        }
        else
        {
            variableStorage.Command.RemoveAll(v => v.Key == variableName);
            variableStorage.Session.RemoveAll(v => v.Key == variableName);
            existingChange = variableStorage.Application.FirstOrDefault(v => v.Key == variableName);
        }

        if (existingChange != null)
        {
            var newList = (existingChange.Value.ListValue?.RemoveAll(v => v.ObjectValue?["key"].TextValue == key) ?? new DynamicValueList()).Add(value);
            existingChange.Value = new DynamicValue(newList);
            return;
        }

        var existingVariable =
            variableStorage.Command.FirstOrDefault(v => v.Key == variableName)
            ?? variableStorage.Session.FirstOrDefault(v => v.Key == variableName)
            ?? variableStorage.Application.FirstOrDefault(v => v.Key == variableName)
            ?? variableStorage.BuiltIn.FirstOrDefault(v => v.Key == variableName);

        var resolvedRepositoryElementId = repositoryElementId ?? existingVariable?.RepositoryElementId;

        var newVariable = new Variable { Key = variableName, Value = new DynamicValue(new DynamicValueList([value])), RepositoryElementId = resolvedRepositoryElementId };
        if (scope == VariableScope.Command) variableStorage.Command.Add(newVariable);
        if (scope == VariableScope.Session) variableStorage.Session.Add(newVariable);
        if (scope == VariableScope.Application) variableStorage.Application.Add(newVariable);
    }
}
