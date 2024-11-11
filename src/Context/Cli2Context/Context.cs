
namespace Cli2Context;

public class Context(IVariables variables, IFlow flow, IEmbeddedRepository embeddedRepository, IRepository repository, ISerializerFactory serializerFactory, IOutput output) : IContext
{
    private readonly ISerializerFactory _serializerFactory = serializerFactory;

    private List<Command> Commands = [];
    private Dictionary<RepositoryLocation, Dictionary<string, RepositoryElementContent>> _originalRepositories = new Dictionary<RepositoryLocation, Dictionary<string, RepositoryElementContent>>();

    public async Task Initialize(CancellationToken cancellationToken = default)
    {
        var builtInElements = embeddedRepository.GetList(cancellationToken);
        await ProcessElements(RepositoryLocation.BuiltIn, null, builtInElements);
        var localElements = repository.GetList(RepositoryLocation.Application, null, cancellationToken);
        await ProcessElements(RepositoryLocation.Application, null, localElements);
        var currentSessionName = variables.CurrentSessionName;
        var sessionElements = repository.GetList(RepositoryLocation.Session, currentSessionName, cancellationToken);
        await ProcessElements(RepositoryLocation.Session, currentSessionName, sessionElements);
    }

    public async Task Run(string commmandName, Dictionary<string, string> arguments, CancellationToken cancellationToken = default)
    {
        //variables.WriteVariableValue(VariableScope.Command, "BuiltInPath", new DynamicValue(_repository.GetPath(RepositoryLocation.BuiltIn)));
        //variables.WriteVariableValue(VariableScope.Command, "LocalPath", new DynamicValue(_repository.GetPath(RepositoryLocation.Application)));
        //variables.WriteVariableValue(VariableScope.Command, "SessionPath", new DynamicValue(_repository.GetPath(RepositoryLocation.Session, variables.CurrentSessionName)));

        if (string.IsNullOrWhiteSpace(commmandName))
        {
            flow.Terminate($"Command not provided.");
            return;
        }

        var command = Commands.FirstOrDefault(c => c.Name == commmandName);
        if (command == null)
        {
            flow.Terminate($"Unknown command {commmandName}");
            return;
        }

        //Convert command parameters (both defined by command and built-in) to variables with values from arguments.
        foreach (var parameter in command.Parameters.Union(BuiltInCommandParameters.List))
        {
            arguments.TryGetValue(parameter.Key, out var argumentValue);
            variables.WriteVariableValue(VariableScope.Command, parameter.Key, new DynamicValue(argumentValue));
        }

        //Check if all required parameters have value
        foreach (var parameter in command.Parameters)
        {
            var variable = variables.FindVariable(parameter.Key);
            var value = variable == null ? null : variables.ReadVariableValue(variable.Value);
            if (parameter.Required && string.IsNullOrWhiteSpace(value?.TextValue))
            {
                flow.Terminate($"Parameter {parameter.Key} requires value.");
                return;
            }
        }

        //Run operations for selected command.
        foreach (var operation in command.Operations)
        {
            output.Debug($"{operation.Name}: Starting");

            //Evaluate conditions.
            //TODO: Add test if all properties are called here.
            if (operation.Conditions.IsNull != null)
            {
                var result = variables.ReadVariableValue(operation.Conditions.IsNull);
                if (!result.IsNull())
                {
                    output.Trace($"Skipping operation {operation.Name} because of IsNull condition.");
                    continue;
                }
            }
            if (operation.Conditions.IsNotNull != null)
            {
                var result = variables.ReadVariableValue(operation.Conditions.IsNotNull);
                if (result.IsNull())
                {
                    output.Trace($"Skipping operation {operation.Name} because of IsNotNull condition.");
                    continue;
                }
            }

            foreach (var parameterKey in operation.Parameters.Keys)
            {
                //Evaluate all operation parametes.
                operation.Parameters[parameterKey] = operation.Parameters[parameterKey] with { Value = variables.ReadVariableValue(operation.Parameters[parameterKey].Value) ?? new DynamicValue() };
                if (operation.Parameters[parameterKey].Required && operation.Parameters[parameterKey].Value.IsNull())
                    flow.Terminate($"Parameter {parameterKey} is required in operation {operation.Name}.");
                if (operation.Parameters[parameterKey].RequiredText && string.IsNullOrWhiteSpace(operation.Parameters[parameterKey].Value.TextValue))
                    flow.Terminate($"Parameter {parameterKey} requires text value in operation {operation.Name}.");

                if (operation.Parameters[parameterKey].AllowedEnumValues != null)
                {
                    if (!operation.Parameters[parameterKey].AllowedEnumValues!.IsEnum)
                        //TODO: This should be checked by unit test.
                        flow.Terminate($"Parameter {parameterKey} contains invalid AllowedEnumValues in operation {operation.Name}.");
                   
                    if (!string.IsNullOrWhiteSpace(operation.Parameters[parameterKey].Value.TextValue) && !Enum.IsDefined(operation.Parameters[parameterKey].AllowedEnumValues!, operation.Parameters[parameterKey].Value.TextValue!))
                        flow.Terminate($"Parameter {parameterKey} has invalid value in operation {operation.Name}."); 
           
                }
            }

            await operation.Run(cancellationToken);
        }

        //Save changes in variables
        var variableList = variables.GetVariableList(RepositoryLocation.Application);
        foreach (var locationId in variableList.Keys)
        {
            var resolvedLocationId = string.IsNullOrWhiteSpace(locationId) ? "variables.json" : locationId;
            var originalFile = _originalRepositories.ContainsKey(RepositoryLocation.Application) && _originalRepositories[RepositoryLocation.Application].ContainsKey(resolvedLocationId)
                        ? _originalRepositories[RepositoryLocation.Application][resolvedLocationId] : null;
            if (originalFile != null)
                originalFile.Variables = variableList[resolvedLocationId].Select(v => new Variable { Key = v.Key, Value = v.Value }).ToList();
            else
                originalFile = new RepositoryElementContent { Variables = variableList[resolvedLocationId].Select(v => new Variable { Key = v.Key, Value = v.Value }).ToList() };

            var serializer = _serializerFactory.GetSerializer("json");
            if (serializer.TrySerialize(originalFile, out var content, out var exception))
                repository.SaveList(RepositoryLocation.Application, resolvedLocationId, null, content, cancellationToken);
        }
    }


    private async Task ProcessElements(RepositoryLocation repositoryLocation, string? sessionName, IAsyncEnumerable<RepositoryElement> elements)
    {
        await foreach (var element in elements)
        {
            variables.CurrentlyLoadRepositoryElement = element.FriendlyName;
            output.Debug($"Processing {repositoryLocation.ToString()} element {element.FriendlyName}");
            //if (element.Exception != null)
            //{
            //    output.Warning($"Failed to load {element.Id}. Exception occurred: {element.Exception.Message}");
            //    continue;
            //}

            var serializer = GetSerializer(element);
            if (serializer == null)
                return;

            //TODO: Deserialization should also return Exception if occurred.
            serializer.TryDeserialize(element.Content, out RepositoryElementContent? contentObject, out var exception);
            if (contentObject == null)
            {
                output.Warning($"Failed to deserialize {element.Id}.");
                return;
            }

            //TODO: if deserialization failed, we have to save this information. Otherwise changes will overwrite whole file.
            if (!_originalRepositories.ContainsKey(repositoryLocation))
                _originalRepositories[repositoryLocation] = new Dictionary<string, RepositoryElementContent>();
            _originalRepositories[repositoryLocation][element.Id] = contentObject;

            if (contentObject.Variables != null)
                variables.Add(repositoryLocation, contentObject.Variables, element.Id);
            if (contentObject.Commands != null)
                Commands.AddRange(contentObject.Commands.Where(c => c != null)!);
        }

        variables.CurrentlyLoadRepositoryElement = null;
    }

    private ISerializer? GetSerializer(RepositoryElement element)
    {
        if (element.Format == null)
        {
            output.Warning($"Failed to determine format of {element.Id}.");
            return null;
        }
        var serializer = _serializerFactory.GetSerializer(element.Format);
        if (serializer == null)
        {
            output.Warning($"Failed to deserialize {element.Id}. Unknown format {element.Format}");
            return null;
        }
        
        return serializer;
    }
}