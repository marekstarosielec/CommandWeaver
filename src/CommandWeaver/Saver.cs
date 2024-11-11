public interface ISaver
{
    void Execute(CancellationToken cancellationToken);
}

public class Saver(IVariables variables) : ISaver
{
    public void Execute(CancellationToken cancellationToken)
    {
        //Save changes in variables
        var variableList = variables.GetVariableList(RepositoryLocation.Application);
        foreach (var locationId in variableList.Keys)
        {
            //    var resolvedLocationId = string.IsNullOrWhiteSpace(locationId) ? "variables.json" : locationId;
            //    var originalFile = _originalRepositories.ContainsKey(RepositoryLocation.Application) && _originalRepositories[RepositoryLocation.Application].ContainsKey(resolvedLocationId)
            //                ? _originalRepositories[RepositoryLocation.Application][resolvedLocationId] : null;
            //    if (originalFile != null)
            //        originalFile.Variables = variableList[resolvedLocationId].Select(v => new Variable { Key = v.Key, Value = v.Value }).ToList();
            //    else
            //        originalFile = new RepositoryElementContent { Variables = variableList[resolvedLocationId].Select(v => new Variable { Key = v.Key, Value = v.Value }).ToList() };

            //    var serializer = _serializerFactory.GetSerializer("json");
            //    if (serializer.TrySerialize(originalFile, out var content, out var exception))
            //        repository.SaveList(RepositoryLocation.Application, resolvedLocationId, null, content, cancellationToken);
        }
    }
}

