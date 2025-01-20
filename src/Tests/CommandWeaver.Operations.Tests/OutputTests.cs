using System.Collections.Immutable;
using NSubstitute;

public class OutputTests
{
    [Fact]
    public async Task Run_ShouldWriteValueWithSpecifiedParameters()
    {
        // Arrange
        var mockOutputService = Substitute.For<IOutputService>();
        var parameters = new Dictionary<string, OperationParameter>
        {
            { "value", new OperationParameter { Value = new DynamicValue("Test Value"), Description = "description"} },
            { "styling", new OperationParameter { Value = new DynamicValue("Markup"), Validation = new Validation { AllowedEnumValues = typeof(Styling)}, Description = "description" }},
            { "logLevel", new OperationParameter { Value = new DynamicValue("Information"), Validation = new Validation { AllowedEnumValues = typeof(LogLevel)}, Description = "description" } }
        }.ToImmutableDictionary();

        var output = new Output(mockOutputService) { Parameters = parameters };

        // Act
        await output.Run(CancellationToken.None);

        // Assert
        mockOutputService.Received(1).Write(
            Arg.Is<DynamicValue>(v => v.TextValue == "Test Value"),
            Arg.Is<LogLevel?>(level => level == LogLevel.Information),
            Arg.Is<Styling>(style => style == Styling.Markup)
        );
    }

    [Fact]
    public async Task Run_ShouldUseDefaultStylingWhenStylingIsNotSpecified()
    {
        // Arrange
        var mockOutputService = Substitute.For<IOutputService>();
        var parameters = new Dictionary<string, OperationParameter>
        {
            { "value", new OperationParameter { Value = new DynamicValue("Test Value"), Description = "description" } },
            { "logLevel", new OperationParameter { Value = new DynamicValue(), Description = "description" } },
            { "styling", new OperationParameter { Value = new DynamicValue(), Validation = new Validation { AllowedEnumValues = typeof(Styling)}, Description = "description" } }
        }.ToImmutableDictionary();

        var output = new Output(mockOutputService) { Parameters = parameters };

        // Act
        await output.Run(CancellationToken.None);

        // Assert
        mockOutputService.Received(1).Write(
            Arg.Is<DynamicValue>(v => v.TextValue == "Test Value"),
            Arg.Any<LogLevel?>(),
            Arg.Is<Styling>(style => style == Styling.Default)
        );
    }
}
