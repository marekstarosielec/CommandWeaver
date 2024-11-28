using NSubstitute;

public class CommandMetadataServiceTests
{
    private readonly IVariableService _variableService = Substitute.For<IVariableService>();
    private readonly IJsonSerializer _serializer = Substitute.For<IJsonSerializer>();
    private readonly IFlowService _flowService = Substitute.For<IFlowService>();
    private readonly IOutputService _outputService = Substitute.For<IOutputService>();

    private readonly CommandMetadataService _service;

    public CommandMetadataServiceTests()
    {
        _service = new CommandMetadataService(_variableService, _serializer, _flowService, _outputService);
    }

    [Fact]
    public void StoreCommandMetadata_ShouldStoreMetadata_WhenSerializationSucceeds()
    {
        // Arrange
        var command = new Command { Name = "test-command" };
        var repositoryElementId = "repo-1";
        var serializedCommand = "{ \"name\": \"test-command\" }";

        _serializer.TryDeserialize(serializedCommand, out Arg.Any<DynamicValue?>(), out Arg.Any<Exception?>())
            .Returns(args =>
            {
                args[1] = new DynamicValue("mocked-definition");
                return true;
            });

        // Act
        _service.StoreCommandMetadata(repositoryElementId, command, serializedCommand);

        // Assert
        _variableService.Received(1).WriteVariableValue(
            VariableScope.Command,
            "commands[test-command]",
            Arg.Is<DynamicValue>(v => 
                v.ObjectValue!.Keys.Contains("key") == true && 
                v.ObjectValue["key"].TextValue == "test-command")
        );
        _outputService.Received(1).Debug("Metadata for command 'test-command' stored with key 'commands[test-command]'.");
    }

    [Fact]
    public void StoreCommandMetadata_ShouldHandleSerializationFailure()
    {
        // Arrange
        var command = new Command { Name = "test-command" };
        var repositoryElementId = "repo-1";
        var serializedCommand = "{ \"name\": \"test-command\" }";

        _serializer.TryDeserialize(serializedCommand, out Arg.Any<DynamicValue?>(), out Arg.Any<Exception?>())
            .Returns(args =>
            {
                args[2] = new Exception("Deserialization error");
                return false;
            });

        // Act
        _service.StoreCommandMetadata(repositoryElementId, command, serializedCommand);

        // Assert
        _variableService.DidNotReceiveWithAnyArgs().WriteVariableValue(Arg.Any<VariableScope>(), Arg.Any<string>(), Arg.Any<DynamicValue>());
        _outputService.Received(1).Warning("Failed to deserialize command metadata for test-command");
    }
}
