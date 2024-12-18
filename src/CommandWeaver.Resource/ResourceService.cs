/// <inheritdoc />
public class ResourceService(IVariableService variableService) : IResourceService
{
    /// <inheritdoc />
    public void Add(RepositoryElementInformation repositoryElementInformation)
    {
        var resourcesList =
            variableService.ReadVariableValue(new DynamicValue("{{ resources }}"))?.ListValue?.ToList() ?? [];
        var resourceObject = new Dictionary<string, DynamicValue?>
        {
            ["key"] = new (repositoryElementInformation.Id),
            ["text"] = repositoryElementInformation.ContentAsString != null ? new DynamicValue(repositoryElementInformation.ContentAsString) : new DynamicValue(),
            ["binary"] = repositoryElementInformation.ContentAsBinary != null ? new DynamicValue(repositoryElementInformation.ContentAsBinary) : new DynamicValue()
        };
        resourcesList.Add(new DynamicValue(new DynamicValueObject(resourceObject)));
        variableService.WriteVariableValue(VariableScope.Command, "resources", new DynamicValue(resourcesList));
    }
}