using CommandLine;
using Models;
using Models.Interfaces.Context;
using Repositories.Abstraction;
using Repositories.Abstraction.Interfaces;
using Serializer.Abstractions;

namespace Cli2Context;

public class Context(IRepository repository, ISerializerFactory serializerFactory, IOutput output) : IContext
{
    public IContextServices Services { get; } = new ContextServices(output);

    public IContextVariables Variables { get; } = new ContextVariables(output);

    public List<Command> Commands { get; } = [];
    
    public async Task Load(CancellationToken cancellationToken = default)
    {
        var builtInElements = repository.GetList(RepositoryLocation.BuiltIn, null, cancellationToken);
        await ProcessElements(RepositoryLocation.BuiltIn, null, builtInElements);
        var localElements = repository.GetList(RepositoryLocation.Local, null, cancellationToken);
        await ProcessElements(RepositoryLocation.Local, null, localElements);
        var currentSessionName = Variables.CurrentSessionName;
        var sessionElements = repository.GetList(RepositoryLocation.Session, currentSessionName, cancellationToken);
        await ProcessElements(RepositoryLocation.Session, currentSessionName, sessionElements);
    }

    public async Task Run(string commandLineArguments, CancellationToken cancellationToken = default)
    {
        var cmd = new Parser().ParseFullCommandLine(commandLineArguments);
        var parsedArguments = cmd.ParsedArguments.ToList();
        var command = Commands.First(c => c.Name == "nag-core-environment-add");
        foreach (var parameter in command.Parameters)
        {
            var t = parsedArguments.FirstOrDefault(p =>
                p.Name.Equals(parameter.Key, StringComparison.InvariantCultureIgnoreCase));
            //What will happen if argument was not provided?
            Variables.SetVariableValue(VariableScope.Command, parameter.Key, t.Value, parameter.Description); 
            //How to validate value?
        }
        await new CommandRunner().RunCommand(command, this, cancellationToken);
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
        var content = await repository.GetContent(repositoryLocation, sessionName, element.Id);
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
        var serializer = serializerFactory.GetSerializer(element.Format);
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
        
        Variables.SetVariableList(repositoryLocation, contentObject);
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