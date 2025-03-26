using System.Collections.Immutable;
using NSubstitute;

public class TerminateTests
{
    [Fact]
    public async Task Run_ShouldCallTerminateWithMessage_WhenMessageIsProvided()
    {
        // Arrange
        var parameters = new Dictionary<string, OperationParameter>
        {
            { "message", new OperationParameter { Value = new DynamicValue("Goodbye!"), Description = "description" } },
            { "exitCode", new OperationParameter { Description = "test" } }
        }.ToImmutableDictionary();

        var terminate = new Terminate() { Parameters = parameters };

        // Act
        await Assert.ThrowsAsync<CommandWeaverException>(async () => await terminate.Run(CancellationToken.None));
    }

    [Fact]
    public async Task Run_ShouldCallTerminateWithEmptyMessage_WhenMessageIsNotProvided()
    {
        // Arrange
        var parameters = new Dictionary<string, OperationParameter>
        {
            { "message", new OperationParameter { Value = new DynamicValue(), Description = "description" } },
            { "exitCode", new OperationParameter { Description = "test" } }
        }.ToImmutableDictionary();

        var terminate = new Terminate() { Parameters = parameters };

        // Act
        var ex = await Assert.ThrowsAsync<CommandWeaverException>(async () => await terminate.Run(CancellationToken.None));
        
        Assert.Empty(ex.Message);
    }
}
