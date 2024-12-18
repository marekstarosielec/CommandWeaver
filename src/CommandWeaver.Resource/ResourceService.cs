using System.Text;

public class ResourceService(IVariableService variableService) : IResourceService
{
    private readonly Dictionary<string, RepositoryElementInformation> _resources = new ();
    
    public void Add(RepositoryElementInformation repositoryElementInformation)
    {
        // using var memoryStream = new MemoryStream();
        // repositoryElementInformation.Stream.CopyTo(memoryStream);
        // var t = memoryStream.ToArray();
        // var t2 = Encoding.UTF8.GetString(t);
        _resources[repositoryElementInformation.Id] = repositoryElementInformation;
        var currentResources = variableService.ReadVariableValue(new DynamicValue("{{ resources }}"));
        var resourcesList = currentResources.ListValue?.ToList() ?? new List<DynamicValue>();
        var resourceObject = new Dictionary<string, DynamicValue?>();
        resourceObject["key"] = new DynamicValue(repositoryElementInformation.Id);
        resourceObject["text"] = new DynamicValue(repositoryElementInformation.ContentAsString);
        resourceObject["binary"] = new DynamicValue(repositoryElementInformation.ContentAsBinary);
        resourcesList.Add(new DynamicValue(new DynamicValueObject(resourceObject)));
        variableService.WriteVariableValue(VariableScope.Command, "resources", new DynamicValue(resourcesList));
    }
}