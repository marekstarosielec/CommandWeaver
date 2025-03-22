using NSubstitute;

public class CommandServiceTests
{
    [Fact]
    public void Add_ShouldAddCommandsToListAndStoreMetadata()
    {
        // Arrange
        var conditionsService = Substitute.For<IConditionsService>();
        var commandMetadataService = Substitute.For<ICommandMetadataService>();
        var operationParameterResolver = Substitute.For<IOperationParameterResolver>();
        var outputService = Substitute.For<IOutputService>();
        var operationFactory = Substitute.For<IOperationFactory>();
        var variableService = Substitute.For<IVariableService>();
        var commandService = new CommandService(conditionsService, commandMetadataService, operationParameterResolver, outputService, operationFactory, variableService);
        const string repositoryElementId = "repo-123";
        var commands = new List<Command>
        {
            new Command { Name = new DynamicValue("cmd1") },
            new Command { Name = new DynamicValue("cmd2") }
        };

        // Act
        commandService.Add(repositoryElementId, commands);

        // Assert
        Assert.NotNull(commandService.Get("cmd1"));
        Assert.NotNull(commandService.Get("cmd2"));

        commandMetadataService.Received(1).StoreCommandMetadata(
            repositoryElementId,
            Arg.Is<Command>(c => c.Name.TextValue == "cmd1"));
        commandMetadataService.Received(1).StoreCommandMetadata(
            repositoryElementId,
            Arg.Is<Command>(c => c.Name.TextValue == "cmd2"));
    }

    [Fact]
    public void Get_ShouldReturnCommandByNameOrAlias()
    {
        // Arrange
        var conditionsService = Substitute.For<IConditionsService>();
        var commandMetadataService = Substitute.For<ICommandMetadataService>();
        var operationParameterResolver = Substitute.For<IOperationParameterResolver>();
        var outputService = Substitute.For<IOutputService>();
        var operationFactory = Substitute.For<IOperationFactory>();
        var variableService = Substitute.For<IVariableService>();
        var commandService = new CommandService(conditionsService, commandMetadataService, operationParameterResolver, outputService, operationFactory, variableService);
        const string repositoryElementId = "repo-123";
        var names = new List<DynamicValue> { new("cmd1"), new("alias1") };
        var commands = new List<Command>
        {
            new Command { Name =  new DynamicValue(names) }
        };

        commandService.Add(repositoryElementId, commands);

        // Act
        var commandByName = commandService.Get("cmd1");
        var commandByAlias = commandService.Get("alias1");

        // Assert
        Assert.NotNull(commandByName);
        Assert.NotNull(commandByAlias);
        Assert.Equal("cmd1", commandByName.GetAllNames().FirstOrDefault());
        Assert.Equal("cmd1", commandByAlias.GetAllNames().FirstOrDefault());
    }

    [Fact]
    public async Task ExecuteOperations_ShouldSkipOperationsWhenConditionsAreNotMet()
    {
        // Arrange
        var conditionsService = Substitute.For<IConditionsService>();
        var commandMetadataService = Substitute.For<ICommandMetadataService>();
        var operationParameterResolver = Substitute.For<IOperationParameterResolver>();
        var outputService = Substitute.For<IOutputService>();
        var operationFactory = Substitute.For<IOperationFactory>();
        var variableService = Substitute.For<IVariableService>();
        variableService.ReadVariableValue(Arg.Any<DynamicValue>()).Returns(new DynamicValue(true));
        conditionsService.ConditionsAreMet(Arg.Any<Condition?>()).Returns(false);
        var commandService = new CommandService(conditionsService, commandMetadataService, operationParameterResolver, outputService, operationFactory, variableService);
        var operations = new List<Operation>
        {
            new TestOperation("Op1"),
            new TestOperation("Op2")
        };

        conditionsService.ConditionsAreMet(Arg.Any<Condition?>()).Returns(false);

        // Act
        await commandService.ExecuteOperations(operations, CancellationToken.None);

        // Assert
        operationParameterResolver.DidNotReceive().PrepareOperationParameters(Arg.Any<Operation>());
        foreach (var operation in operations)
            await operation.Run(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteOperations_ShouldCancelExecutionWhenCancellationTokenIsTriggered()
    {
        // Arrange
        var conditionsService = Substitute.For<IConditionsService>();
        var commandMetadataService = Substitute.For<ICommandMetadataService>();
        var operationParameterResolver = Substitute.For<IOperationParameterResolver>();
        var outputService = Substitute.For<IOutputService>();
        var operationFactory = Substitute.For<IOperationFactory>();
        var variableService = Substitute.For<IVariableService>();
        variableService.ReadVariableValue(Arg.Any<DynamicValue>()).Returns(new DynamicValue(true));
        conditionsService.ConditionsAreMet(Arg.Any<Condition?>()).Returns(true);
        var commandService = new CommandService(conditionsService, commandMetadataService, operationParameterResolver, outputService, operationFactory, variableService);
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var operations = new List<Operation>
        {
            new TestOperation("Op1"),
            new TestOperation("Op2")
        };

        foreach (var operation in operations)
            operationParameterResolver.PrepareOperationParameters(operation).Returns(operation);

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await commandService.ExecuteOperations(operations, cts.Token));

        // Assert
        operationParameterResolver.DidNotReceive().PrepareOperationParameters(Arg.Any<Operation>());
        foreach (var operation in operations)
            await operation.Run(cts.Token);
    }
}


