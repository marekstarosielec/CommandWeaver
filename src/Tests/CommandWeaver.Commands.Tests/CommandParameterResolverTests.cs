using System.Collections.Immutable;
using NSubstitute;

public class CommandParameterResolverTests
{
    private readonly IFlowService _flowService = Substitute.For<IFlowService>();
    private readonly IOutputService _outputService = Substitute.For<IOutputService>();
    private readonly IInputService _inputService = Substitute.For<IInputService>();
    private readonly IVariableService _variableService = Substitute.For<IVariableService>();
    private readonly CommandParameterResolver _resolver;

    public CommandParameterResolverTests()
    {
        _resolver = new CommandParameterResolver(_flowService, _outputService, _inputService, _variableService);
    }

    [Fact]
    public void PrepareCommandParameters_ShouldWriteResolvedParametersToVariables()
    {
        // Arrange
        var command = new Command
        {
            Name = "test-command",
            Parameters =
            [
                new CommandParameter { Key = "param1", Required = true },
                new CommandParameter { Key = "param2" }
            ]
        };

        var arguments = new Dictionary<string, string>
        {
            { "param1", "value1" },
            { "param2", "value2" }
        };

        // Act
        _resolver.PrepareCommandParameters(command, arguments);

        // Assert
        _variableService.Received(1).WriteVariableValue(VariableScope.Command, "param1", Arg.Is<DynamicValue>(v => v.TextValue == "value1"));
        _variableService.Received(1).WriteVariableValue(VariableScope.Command, "param2", Arg.Is<DynamicValue>(v => v.TextValue == "value2"));
    }

    [Fact]
    public void PrepareCommandParameters_ShouldResolveFromOtherNames()
    {
        // Arrange
        var command = new Command
        {
            Name = "test-command",
            Parameters =
            [
                new CommandParameter
                {
                    Key = "param1", OtherNames = new List<string> { "alias1", "alias2" }.ToImmutableList(),
                    Required = true
                }
            ]
        };

        var arguments = new Dictionary<string, string>
        {
            { "alias2", "resolvedValue" }
        };

        // Act
        _resolver.PrepareCommandParameters(command, arguments);

        // Assert
        _variableService.Received(1).WriteVariableValue(VariableScope.Command, "param1", Arg.Is<DynamicValue>(v => v.TextValue == "resolvedValue"));
    }

    [Fact]
    public void PrepareCommandParameters_ShouldTerminateForMissingRequiredParameter()
    {
        // Arrange
        var command = new Command
        {
            Name = "test-command",
            Parameters = [new CommandParameter { Key = "param1", Required = true }]
        };

        var arguments = new Dictionary<string, string>();

        // Act
        _resolver.PrepareCommandParameters(command, arguments);

        // Assert
        _flowService.Received(1).Terminate(Arg.Is<string>(msg => msg.Contains("param1")));
    }

    [Fact]
    public void PrepareCommandParameters_ShouldValidateAllowedValues()
    {
        // Arrange
        var command = new Command
        {
            Name = "test-command",
            Parameters =
            [
                new CommandParameter
                    { Key = "param1", AllowedValues = new List<string> { "allowed1", "allowed2" }.ToImmutableList() }
            ]
        };

        var arguments = new Dictionary<string, string>
        {
            { "param1", "invalidValue" }
        };

        // Act
        _resolver.PrepareCommandParameters(command, arguments);

        // Assert
        _flowService.Received(1).Terminate(Arg.Is<string>(msg => msg.Contains("Invalid value for argument 'param1'")));
    }

    [Fact]
    public void PrepareCommandParameters_ShouldValidateAllowedEnumValues()
    {
        // Arrange
        var command = new Command
        {
            Name = "test-command",
            Parameters = [new CommandParameter(key: "param1", allowedEnumValues: typeof(LogLevel))]
        };

        var arguments = new Dictionary<string, string>
        {
            { "param1", "InvalidEnumValue" }
        };

        // Act
        _resolver.PrepareCommandParameters(command, arguments);

        // Assert
        _flowService.Received(1).Terminate(Arg.Is<string>(msg => msg.Contains("Invalid value for argument 'param1'")));
    }

    [Fact]
    public void PrepareCommandParameters_ShouldNotTerminateForValidEnumValue()
    {
        // Arrange
        var command = new Command
        {
            Name = "test-command",
            Parameters = [new CommandParameter { Key = "param1", AllowedEnumValues = typeof(LogLevel) }]
        };

        var arguments = new Dictionary<string, string>
        {
            { "param1", "Trace" }
        };

        // Act
        _resolver.PrepareCommandParameters(command, arguments);

        // Assert
        _flowService.DidNotReceive().Terminate(Arg.Any<string>());
        _variableService.Received(1).WriteVariableValue(VariableScope.Command, "param1", Arg.Is<DynamicValue>(v => v.TextValue == "Trace"));
    }
    
    [Fact]
    public void PrepareCommandParameters_UsesIfNullTextValue()
    {
        // Arrange
        var command = new Command
        {
            Name = "test-command",
            Parameters =
            [
                new CommandParameter { Key = "param1", IfNull = new DynamicValue("abc")},
            ]
        };
        _variableService.ReadVariableValue(Arg.Is<DynamicValue>(v => v.TextValue == "abc")).Returns(new DynamicValue("abc"));
        var arguments = new Dictionary<string, string>();

        // Act
        _resolver.PrepareCommandParameters(command, arguments);

        // Assert
        _variableService.Received(1).WriteVariableValue(VariableScope.Command, "param1", Arg.Is<DynamicValue>(v => v.TextValue == "abc"));
    }
    
    [Fact]
    public void PrepareCommandParameters_UsesIfNullListValue()
    {
        // Arrange
        var command = new Command
        {
            Name = "test-command",
            Parameters =
            [
                new CommandParameter { Key = "param1", IfNull = new DynamicValue(new List<DynamicValue>
                {
                    new ("{{abc}}"),
                    new (123)
                })},
            ]
        };
        _variableService.ReadVariableValue(Arg.Is<DynamicValue>(v => v.ListValue!.First().TextValue == "{{abc}}" && v.ListValue!.Last().NumericValue == 123)).Returns(new DynamicValue(123));
        var arguments = new Dictionary<string, string>();

        // Act
        _resolver.PrepareCommandParameters(command, arguments);

        // Assert
        _variableService.Received(1).WriteVariableValue(VariableScope.Command, "param1", Arg.Is<DynamicValue>(v => v.TextValue == "123"));
    }
}
