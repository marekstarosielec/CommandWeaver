// using NSubstitute;
// using System.Collections.Immutable;
//
// public class CommandServiceTests
// {
//     [Fact]
//     public void Add_ShouldAddCommandsToListAndStoreMetadata()
//     {
//         // Arrange
//         var conditionsService = Substitute.For<IConditionsService>();
//         var commandMetadataService = Substitute.For<ICommandMetadataService>();
//         var operationParameterResolver = Substitute.For<IOperationParameterResolver>();
//         var outputService = Substitute.For<IOutputService>();
//         var commandService = new CommandService(conditionsService, commandMetadataService, operationParameterResolver, outputService);
//         const string repositoryElementId = "repo-123";
//         var commands = new List<Command>
//         {
//             new Command { Name = "cmd1" },
//             new Command { Name = "cmd2" }
//         };
//
//         // Act
//         commandService.Add(repositoryElementId, commands);
//
//         // Assert
//         Assert.NotNull(commandService.Get("cmd1"));
//         Assert.NotNull(commandService.Get("cmd2"));
//
//         commandMetadataService.Received(1).StoreCommandMetadata(
//             repositoryElementId,
//             Arg.Is<Command>(c => c.Name == "cmd1"));
//         commandMetadataService.Received(1).StoreCommandMetadata(
//             repositoryElementId,
//             Arg.Is<Command>(c => c.Name == "cmd2"));
//     }
//
//     [Fact]
//     public void Get_ShouldReturnCommandByNameOrAlias()
//     {
//         // Arrange
//         var conditionsService = Substitute.For<IConditionsService>();
//         var commandMetadataService = Substitute.For<ICommandMetadataService>();
//         var operationParameterResolver = Substitute.For<IOperationParameterResolver>();
//         var outputService = Substitute.For<IOutputService>();
//         var commandService = new CommandService(conditionsService, commandMetadataService, operationParameterResolver, outputService);
//         const string repositoryElementId = "repo-123";
//         var commands = new List<Command>
//         {
//             new Command { Name = "cmd1", OtherNames = ["alias1"] }
//         };
//
//         commandService.Add(repositoryElementId, commands);
//
//         // Act
//         var commandByName = commandService.Get("cmd1");
//         var commandByAlias = commandService.Get("alias1");
//
//         // Assert
//         Assert.NotNull(commandByName);
//         Assert.NotNull(commandByAlias);
//         Assert.Equal("cmd1", commandByName.Name);
//         Assert.Equal("cmd1", commandByAlias.Name);
//     }
//
//     [Fact]
//     public async Task ExecuteOperations_ShouldSkipOperationsWhenConditionsAreNotMet()
//     {
//         // Arrange
//         var conditionsService = Substitute.For<IConditionsService>();
//         var commandMetadataService = Substitute.For<ICommandMetadataService>();
//         var operationParameterResolver = Substitute.For<IOperationParameterResolver>();
//         var outputService = Substitute.For<IOutputService>();
//         var commandService = new CommandService(conditionsService, commandMetadataService, operationParameterResolver, outputService);
//         var operations = new List<Operation>
//         {
//             new TestOperation("Op1"),
//             new TestOperation("Op2")
//         };
//
//         conditionsService.ConditionsAreMet(Arg.Any<Condition?>()).Returns(false);
//
//         // Act
//         await commandService.ExecuteOperations(operations, CancellationToken.None);
//
//         // Assert
//         operationParameterResolver.DidNotReceive().PrepareOperationParameters(Arg.Any<Operation>());
//         foreach (var operation in operations)
//             await operation.Run(Arg.Any<CancellationToken>());
//     }
//
//     [Fact]
//     public async Task ExecuteOperations_ShouldCancelExecutionWhenCancellationTokenIsTriggered()
//     {
//         // Arrange
//         var conditionsService = Substitute.For<IConditionsService>();
//         var commandMetadataService = Substitute.For<ICommandMetadataService>();
//         var operationParameterResolver = Substitute.For<IOperationParameterResolver>();
//         var outputService = Substitute.For<IOutputService>();
//         var commandService = new CommandService(conditionsService, commandMetadataService, operationParameterResolver, outputService);
//         var cts = new CancellationTokenSource();
//         cts.Cancel();
//         var operations = new List<Operation>
//         {
//             new TestOperation("Op1"),
//             new TestOperation("Op2")
//         };
//
//         foreach (var operation in operations)
//             operationParameterResolver.PrepareOperationParameters(operation).Returns(operation);
//
//         // Act
//         await commandService.ExecuteOperations(operations, cts.Token);
//
//         // Assert
//         operationParameterResolver.DidNotReceive().PrepareOperationParameters(Arg.Any<Operation>());
//         foreach (var operation in operations)
//             await operation.Run(cts.Token);
//     }
// }
//
//
