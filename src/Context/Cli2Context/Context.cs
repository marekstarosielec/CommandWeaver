using Repositories.Abstraction;

namespace Cli2Context;

public class Context : IContext
{
    private readonly IRepository _repository;
    private readonly ISerializerFactory _serializerFactory;
   
    //public IContextServices Services { get; }

    public IVariables Variables { get; }
    private IFlow _flow;
    private IOutput _output;
    public List<Command> Commands { get; } = [];

    private Dictionary<RepositoryLocation, Dictionary<string, RepositoryContent>> _originalRepositories = new Dictionary<RepositoryLocation, Dictionary<string, RepositoryContent>>();

    public Context(IVariables variables, IFlow flow, IRepository repository, ISerializerFactory serializerFactory, IOutput output)
    {
        _repository = repository;
        _serializerFactory = serializerFactory;
        //Services = new ContextServices(output);
        Variables = variables;
        _flow = flow;
        _output = output;
    } 

    public async Task Initialize(CancellationToken cancellationToken = default)
    {
        var builtInElements = _repository.GetList(RepositoryLocation.BuiltIn, null, cancellationToken);
        await ProcessElements(RepositoryLocation.BuiltIn, null, builtInElements);
        var localElements = _repository.GetList(RepositoryLocation.Application, null, cancellationToken);
        await ProcessElements(RepositoryLocation.Application, null, localElements);
        var currentSessionName = Variables.CurrentSessionName;
        var sessionElements = _repository.GetList(RepositoryLocation.Session, currentSessionName, cancellationToken);
        await ProcessElements(RepositoryLocation.Session, currentSessionName, sessionElements);
    }

    public async Task Run(string commmandName, Dictionary<string, string> arguments, CancellationToken cancellationToken = default)
    {
        Variables.WriteVariableValue(VariableScope.Command, "BuiltInPath", new DynamicValue(_repository.GetPath(RepositoryLocation.BuiltIn)));
        Variables.WriteVariableValue(VariableScope.Command, "LocalPath", new DynamicValue(_repository.GetPath(RepositoryLocation.Application)));
        Variables.WriteVariableValue(VariableScope.Command, "SessionPath", new DynamicValue(_repository.GetPath(RepositoryLocation.Session, Variables.CurrentSessionName)));

        if (string.IsNullOrWhiteSpace(commmandName))
        {
            Terminate($"Command not provided.");
            return;
        }

        var command = Commands.FirstOrDefault(c => c.Name == commmandName);
        if (command == null)
        {
            Terminate($"Unknown command {commmandName}");
            return;
        }

        //Conver command parameters (both defined by command and built-in) to variables with values from arguments.
        foreach (var parameter in command.Parameters.Union(BuiltInCommandParameters.List))
        {
            arguments.TryGetValue(parameter.Key, out var argumentValue);
            Variables.WriteVariableValue(VariableScope.Command, parameter.Key, new DynamicValue(argumentValue));
        }

        //Check if all required parameters have value
        foreach (var parameter in command.Parameters)
        {
            var variable = Variables.FindVariable(parameter.Key);
            var value = variable == null ? null : Variables.ReadVariableValue(variable.Value);
            if (parameter.Required && string.IsNullOrWhiteSpace(value?.TextValue))
            {
                Terminate($"Parameter {parameter.Key} requires value.");
                return;
            }
        }

        //Run operations for selected command.
        foreach (var operation in command.Operations)
        {
            _output.Debug($"{operation.Name}: Starting");

            //Evaluate conditions.
            //TODO: Add test if all properties are called here.
            if (operation.Conditions.IsNull != null)
            {
                var result = Variables.ReadVariableValue(operation.Conditions.IsNull);
                if (!result.IsNull())
                {
                    _output.Trace($"Skipping operation {operation.Name} because of IsNull condition.");
                    continue;
                }
            }
            if (operation.Conditions.IsNotNull != null)
            {
                var result = Variables.ReadVariableValue(operation.Conditions.IsNotNull);
                if (result.IsNull())
                {
                    _output.Trace($"Skipping operation {operation.Name} because of IsNotNull condition.");
                    continue;
                }
            }

            foreach (var parameterKey in operation.Parameters.Keys)
            {
                //Evaluate all operation parametes.
                operation.Parameters[parameterKey] = operation.Parameters[parameterKey] with { Value = Variables.ReadVariableValue(operation.Parameters[parameterKey].Value) ?? new DynamicValue() };
                if (operation.Parameters[parameterKey].Required && operation.Parameters[parameterKey].Value.IsNull())
                    Terminate($"Parameter {parameterKey} is required in operation {operation.Name}.");
                if (operation.Parameters[parameterKey].RequiredText && string.IsNullOrWhiteSpace(operation.Parameters[parameterKey].Value.TextValue))
                    Terminate($"Parameter {parameterKey} requires text value in operation {operation.Name}.");

                if (operation.Parameters[parameterKey].AllowedEnumValues != null)
                {
                    if (!operation.Parameters[parameterKey].AllowedEnumValues!.IsEnum)
                        //TODO: This should be checked by unit test.
                        Terminate($"Parameter {parameterKey} contains invalid AllowedEnumValues in operation {operation.Name}.");
                   
                    if (!string.IsNullOrWhiteSpace(operation.Parameters[parameterKey].Value.TextValue) && !Enum.IsDefined(operation.Parameters[parameterKey].AllowedEnumValues!, operation.Parameters[parameterKey].Value.TextValue!))
                        Terminate($"Parameter {parameterKey} has invalid value in operation {operation.Name}."); 
           
                }
            }

            await operation.Run(cancellationToken);
        }

        //Save changes in variables
        var variables = Variables.GetVariableList(RepositoryLocation.Application);
        foreach (var locationId in variables.Keys)
        {
            var resolvedLocationId = string.IsNullOrWhiteSpace(locationId) ? "variables.json" : locationId;
            var originalFile = _originalRepositories.ContainsKey(RepositoryLocation.Application) && _originalRepositories[RepositoryLocation.Application].ContainsKey(resolvedLocationId)
                        ? _originalRepositories[RepositoryLocation.Application][resolvedLocationId] : null;
            if (originalFile != null)
                originalFile.Variables = variables[resolvedLocationId].Select(v => new Variable { Key = v.Key, Value = v.Value }).ToList();
            else
                originalFile = new RepositoryContent { Variables = variables[resolvedLocationId].Select(v =>new Variable { Key=v.Key, Value = v.Value}).ToList() };

            var serializer = _serializerFactory.GetSerializer("json");
            if (serializer.TrySerialize(originalFile, out var content, out var exception))
                _repository.SaveList(RepositoryLocation.Application, resolvedLocationId, null, content, cancellationToken);
        }
    }

    public void Terminate(string? message = null, int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(message))
            _output.Error(message);
        //should save variables here?
        Environment.Exit(exitCode);
    }

    private async Task ProcessElements(RepositoryLocation repositoryLocation, string? sessionName, IAsyncEnumerable<RepositoryElementInfo> elements)
    {
        await foreach (var element in elements)
        {
            Variables.CurrentlyProcessedElement = element.FriendlyName;
            _output.Debug($"Processing {repositoryLocation.ToString()} element {element.FriendlyName}");
            if (element.Exception != null)
            {
                _output.Warning($"Failed to load {element.Id}. Exception occurred: {element.Exception.Message}");
                continue;
            }

            var serializer = GetSerializer(repositoryLocation, sessionName, element);
            if (serializer == null)
                return;

            //TODO: Deserialization should also return Exception if occurred.
            serializer.TryDeserialize(element.Content, out RepositoryContent? contentObject, out var exception);
            if (contentObject == null)
            {
                _output.Warning($"Failed to deserialize {element.Id}.");
                return;
            }

            //TODO: if deserialization failed, we have to save this information. Otherwise changes will overwrite whole file.
            if (!_originalRepositories.ContainsKey(repositoryLocation))
                _originalRepositories[repositoryLocation] = new Dictionary<string, RepositoryContent>();
            _originalRepositories[repositoryLocation][element.Id] = contentObject;

            if (contentObject.Variables != null)
                Variables.SetVariableList(repositoryLocation, contentObject.Variables, element.Id);
            if (contentObject.Commands != null)
                Commands.AddRange(contentObject.Commands.Where(c => c != null)!);
        }

        Variables.CurrentlyProcessedElement = null;
    }

    private ISerializer? GetSerializer(RepositoryLocation repositoryLocation, string? sessionName, RepositoryElementInfo element)
    {
        if (element.Format == null)
        {
            _output.Warning($"Failed to determine format of {element.Id}.");
            return null;
        }
        var serializer = _serializerFactory.GetSerializer(element.Format);
        if (serializer == null)
        {
            _output.Warning($"Failed to deserialize {element.Id}. Unknown format {element.Format}");
            return null;
        }
        
        return serializer;
    }
}