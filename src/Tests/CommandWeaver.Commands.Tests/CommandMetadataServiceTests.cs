using NSubstitute;

public class CommandMetadataServiceTests
{
    private readonly IVariableService _variableService = Substitute.For<IVariableService>();
    private readonly IOutputService _outputService = Substitute.For<IOutputService>();
    private readonly ICommandParameterResolver _commandParameterResolver = Substitute.For<ICommandParameterResolver>();

    private readonly CommandMetadataService _service;

    public CommandMetadataServiceTests()
    {
        _service = new CommandMetadataService(_variableService, _outputService, _commandParameterResolver);
    }

    [Fact]
    public void StoreCommandMetadata_ShouldStoreMetadata()
    {
        // Arrange
        var repositoryElementId = "repo-1";
        var source = "{ \"name\": \"test-command\" }";
        var definition = new DynamicValue("mocked-definition");
        var command = new Command { Name = new DynamicValue("test-command"), Source = source, Definition = definition };
        _commandParameterResolver.GetCommandParameters(command).Returns([]);
 
        // Act
        _service.StoreCommandMetadata(repositoryElementId, command);

        // Assert
        _variableService.Received(1).WriteVariableValue(
            VariableScope.Command,
            "commands[test-command]",
            Arg.Is<DynamicValue>(v => 
                v.ObjectValue!.Keys.Contains("key") == true 
                && v.ObjectValue["key"].TextValue == "test-command"
                && v.ObjectValue["source"].TextValue == source)
        );
        _outputService.Received(1).Debug("Metadata for command 'test-command' stored with key 'commands[test-command]'.");
    }
}
