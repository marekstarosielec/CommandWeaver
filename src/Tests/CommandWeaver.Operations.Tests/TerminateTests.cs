using System.Collections.Immutable;
using NSubstitute;

public class TerminateTests
{
    [Fact]
    public async Task Run_ShouldCallTerminateWithMessage_WhenMessageIsProvided()
    {
        // Arrange
        var mockFlowService = Substitute.For<IFlowService>();
        var parameters = new Dictionary<string, OperationParameter>
        {
            { "message", new OperationParameter { Value = new DynamicValue("Goodbye!"), Description = "description" } }
        }.ToImmutableDictionary();

        var terminate = new Terminate(mockFlowService) { Parameters = parameters };

        // Act
        await terminate.Run(CancellationToken.None);

        // Assert
        mockFlowService.Received(1).Terminate(Arg.Is<string?>(message => message == "Goodbye!"));
    }

    [Fact]
    public async Task Run_ShouldCallTerminateWithNullMessage_WhenMessageIsNotProvided()
    {
        // Arrange
        var mockFlowService = Substitute.For<IFlowService>();
        var parameters = new Dictionary<string, OperationParameter>
        {
            { "message", new OperationParameter { Value = new DynamicValue(), Description = "description" } }
        }.ToImmutableDictionary();

        var terminate = new Terminate(mockFlowService) { Parameters = parameters };

        // Act
        await terminate.Run(CancellationToken.None);

        // Assert
        mockFlowService.Received(1).Terminate(Arg.Is<string?>(message => message == null));
    }

    [Fact]
    public async Task Run_ShouldCallTerminateWithNull_WhenMessageIsEmpty()
    {
        // Arrange
        var mockFlowService = Substitute.For<IFlowService>();
        var parameters = new Dictionary<string, OperationParameter>
        {
            { "message", new OperationParameter { Value = new DynamicValue(), Description = "description" } }
        }.ToImmutableDictionary();

        var terminate = new Terminate(mockFlowService) { Parameters = parameters };

        // Act
        await terminate.Run(CancellationToken.None);

        // Assert
        mockFlowService.Received(1).Terminate(Arg.Is<string?>(message => message == null));
    }
    
}
