public class ResourceService(IVariableService variableService) : IResourceService
{
    private readonly Dictionary<string, RepositoryElementInformation> _resources = new ();
    
    public void Add(RepositoryElementInformation repositoryElementInformation)
    {
        _resources[repositoryElementInformation.Id] = repositoryElementInformation;
        var dynamicList = new DynamicValueList(_resources.Select(r => new DynamicValue(r.Value.FriendlyName)).ToList());
        variableService.WriteVariableValue(VariableScope.Command, "resources", new DynamicValue(dynamicList));
    }
}