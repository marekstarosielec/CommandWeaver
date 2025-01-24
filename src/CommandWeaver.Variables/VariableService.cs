using System.Collections.Immutable;

/// <inheritdoc />
public class VariableService(IReader reader, IWriter writer, IVariableStorage variableStorage) : IVariableService
{
    /// <inheritdoc />
    public string CurrentSessionName
    {
        get => reader.ReadVariableValue(new DynamicValue("current-session-name"), true)?.TextValue ?? "session1";
        set => WriteVariableValue(VariableScope.Application, "current-session-name", new DynamicValue(value));
    }

    /// <inheritdoc />
    public string? CurrentlyLoadRepository
    {
        get => reader.ReadVariableValue(new DynamicValue("currently-load-repository"), true)?.TextValue;
        set => WriteVariableValue(VariableScope.Command, "currently-load-repository", new DynamicValue(value));
    }

    /// <inheritdoc />
    public string? CurrentlyLoadRepositoryElement
    {
        get => reader.ReadVariableValue(new DynamicValue("currently-load-repository-element"), true)?.TextValue;
        set => WriteVariableValue(VariableScope.Command, "currently-load-repository-element", new DynamicValue(value));
    }

    /// <inheritdoc />
    public LogLevel LogLevel
    {
        get => reader.ReadVariableValue(new DynamicValue("log-level"), true).GetEnumValue<LogLevel>() ?? LogLevel.Information;
        set => WriteVariableValue(VariableScope.Command, "log-level", new DynamicValue(value.ToString()));
    }

    /// <inheritdoc />
    public DynamicValue ReadVariableValue(DynamicValue variableValue, bool treatTextValueAsVariable = false)
    =>   reader.ReadVariableValue(variableValue, treatTextValueAsVariable);

    /// <inheritdoc />
    public void Add(RepositoryLocation repositoryLocation, string repositoryElementId, IEnumerable<Variable> variables)
    {
        //Add repository element id, it will be useful to save changes back.
        var elementsToImport = variables.Select(v => v with { RepositoryElementId = repositoryElementId });

        switch (repositoryLocation)
        {
            case RepositoryLocation.BuiltIn:
                variableStorage.BuiltIn = variableStorage.BuiltIn.AddRange(elementsToImport).ToImmutableList();
                break;
            case RepositoryLocation.Application:
                variableStorage.Application.AddRange(elementsToImport);
                break;
            case RepositoryLocation.Session:
                variableStorage.Session.AddRange(elementsToImport);
                break;
            default:
                throw new InvalidOperationException($"Unknown repository location: {repositoryLocation}");
        }
    }

    /// <inheritdoc />
    public void WriteVariableValue(VariableScope scope, string path, DynamicValue value, string? repositoryElementId = null)
    {
        //Set default name
        if (scope != VariableScope.Command && string.IsNullOrWhiteSpace(repositoryElementId))
            repositoryElementId = Path.ChangeExtension("variables", JsonHelper.Extension);
        
        if (repositoryElementId != null && Path.GetExtension(repositoryElementId).Trim('.').ToLower() !=
            JsonHelper.Extension.ToLower())
            repositoryElementId = Path.ChangeExtension(repositoryElementId, JsonHelper.Extension);
        writer.WriteVariableValue(scope, CurrentSessionName, path, value, repositoryElementId);
    }

    /// <inheritdoc />
    public bool IsVariable(string text)
        => ValuePath.WholePathIsSingleVariable(text);
}
