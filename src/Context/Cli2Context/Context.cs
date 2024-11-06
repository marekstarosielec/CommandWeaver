﻿using Models;
using Models.Interfaces.Context;
using Repositories.Abstraction;
using Repositories.Abstraction.Interfaces;
using Serializer.Abstractions;

namespace Cli2Context;

public class Context : IContext
{
    private readonly IRepository _repository;
    private readonly ISerializerFactory _serializerFactory;
   
    public IContextServices Services { get; }

    public IContextVariables Variables { get; }

    public List<Command> Commands { get; } = [];

    public Context(IRepository repository, ISerializerFactory serializerFactory, IOutput output)
    {
        _repository = repository;
        _serializerFactory = serializerFactory;
        Services = new ContextServices(output);
        Variables = new ContextVariables(this);
    }

    public async Task Initialize(CancellationToken cancellationToken = default)
    {
        var builtInElements = _repository.GetList(RepositoryLocation.BuiltIn, null, cancellationToken);
        await ProcessElements(RepositoryLocation.BuiltIn, null, builtInElements);
        var localElements = _repository.GetList(RepositoryLocation.Local, null, cancellationToken);
        await ProcessElements(RepositoryLocation.Local, null, localElements);
        var currentSessionName = Variables.CurrentSessionName;
        var sessionElements = _repository.GetList(RepositoryLocation.Session, currentSessionName, cancellationToken);
        await ProcessElements(RepositoryLocation.Session, currentSessionName, sessionElements);
    }

    public async Task Run(string commmandName, Dictionary<string, string> arguments, CancellationToken cancellationToken = default)
    {
        Variables.WriteVariableValue(VariableScope.Command, "BuiltInPath", new DynamicValue(_repository.GetPath(RepositoryLocation.BuiltIn)));
        Variables.WriteVariableValue(VariableScope.Command, "LocalPath", new DynamicValue(_repository.GetPath(RepositoryLocation.Local)));
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
            //parameter is different variable - need to find variable with same name and read its value.
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
            Services.Output.Debug($"{operation.Name}: Starting");

            //Evaluate conditions.
            //TODO: Add test if all properties are called here.
            if (operation.Conditions.IsNull != null)
            {
                var result = Variables.ReadVariableValue(operation.Conditions.IsNull);
                if (!result.IsNull)
                {
                    Services.Output.Trace($"Skipping operation {operation.Name} because of IsNull condition.");
                    continue;
                }
            }
            if (operation.Conditions.IsNotNull != null)
            {
                var result = Variables.ReadVariableValue(operation.Conditions.IsNotNull);
                if (result.IsNull)
                {
                    Services.Output.Trace($"Skipping operation {operation.Name} because of IsNotNull condition.");
                    continue;
                }
            }

            foreach (var parameterKey in operation.Parameters.Keys)
            {
                //Evaluate all operation parametes.
                operation.Parameters[parameterKey] = operation.Parameters[parameterKey] with { Value = Variables.ReadVariableValue(operation.Parameters[parameterKey].Value) ?? new DynamicValue() };
                if (operation.Parameters[parameterKey].Required && operation.Parameters[parameterKey].Value.IsNull)
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

            await operation.Run(this, cancellationToken);
            if (cancellationToken.IsCancellationRequested) 
                return;
        }

        //Save changes in variables
    }

    public void Terminate(string? message = null, int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(message)) 
            Services.Output.Error(message);
        Environment.Exit(exitCode);
    }

    private async Task ProcessElements(RepositoryLocation repositoryLocation, string? sessionName, IAsyncEnumerable<RepositoryElementInfo> elements)
    {
        await foreach (var element in elements)
        {
            Variables.CurrentlyProcessedElement = element.FriendlyName;
            Services.Output.Debug($"Processing {repositoryLocation.ToString()} element {element.FriendlyName}");
            if (element.Exception != null)
            {
                Services.Output.Warning($"Failed to load {element.Id}. Exception occurred: {element.Exception.Message}");
                continue;
            }
            
            if (element.Type?.Equals("commands", StringComparison.InvariantCultureIgnoreCase) == true)
                await ReadCommands(repositoryLocation, sessionName, element);
            else if (element.Type?.Equals("variables", StringComparison.InvariantCultureIgnoreCase) == true)
                await ReadVariables(repositoryLocation, sessionName, element);
            else
                Services.Output.Warning($"Failed to load {element.Id}. Unknown type {element.Type}.");
        }

        Variables.CurrentlyProcessedElement = null;
    }

    private async Task<string?> ReadElementContent(RepositoryLocation repositoryLocation, string? sessionName,
        RepositoryElementInfo element)
    {
        var content = await _repository.GetContent(repositoryLocation, sessionName, element.Id);
        if (content.Content == null)
        {
            Services.Output.Warning($"Failed to load content of {element.Id}.");
            return null;
        }

        if (content.Exception != null)
        {
            Services.Output.Warning($"Failed to load content of {element.Id}.  Exception occurred: {content.Exception.Message}");
            return null;
        }

        return content.Content;
    }

    private ISerializer? GetSerializer(RepositoryLocation repositoryLocation, string? sessionName, RepositoryElementInfo element)
    {
        if (element.Format == null)
        {
            Services.Output.Warning($"Failed to determine format of {element.Id}.");
            return null;
        }
        var serializer = _serializerFactory.GetSerializer(element.Format);
        if (serializer == null)
        {
            Services.Output.Warning($"Failed to deserialize {element.Id}. Unknown format {element.Format}");
            return null;
        }
        
        return serializer;
    }
    
    private async Task ReadVariables(RepositoryLocation repositoryLocation, string? sessionName, RepositoryElementInfo element)
    {
        var serializer = GetSerializer(repositoryLocation, sessionName, element);
        if (serializer == null)
            return;
        var content = await ReadElementContent(repositoryLocation, sessionName, element);
        if (content == null)
            return;

        //TODO: Deserialization should also return Exception if occurred.
        var contentObject = serializer.Deserialize<List<Variable?>>(content);
        if (contentObject == null)
        {
            Services.Output.Warning($"Failed to deserialize {element.Id}.");
            return;
        }
        //TODO: Pass id to variable, so the changes can be written in the same file name (but in location/session folder)
        
        Variables.SetVariableList(repositoryLocation, contentObject, element.Id);
    }
    
    private async Task ReadCommands(RepositoryLocation repositoryLocation, string? sessionName, RepositoryElementInfo element)
    {
        var content = await ReadElementContent(repositoryLocation, sessionName, element);
        if (content == null)
            return;

        var serializer = GetSerializer(repositoryLocation, sessionName, element);
        if (serializer == null)
            return;

        //TODO: Deserialization should also return Exception if occurred.
        var command = serializer.Deserialize<Command>(content);
        if (command == null)
        {
            Services.Output.Warning($"Failed to deserialize {element.Id}.");
            return;
        }
        
        Commands.Add(command);
    }
}