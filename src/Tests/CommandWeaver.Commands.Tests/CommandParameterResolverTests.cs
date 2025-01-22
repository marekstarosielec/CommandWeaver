using System.Collections.Immutable;
using NSubstitute;

public class CommandParameterResolverTests
{
    private readonly IOutputService _outputService = Substitute.For<IOutputService>();
    private readonly IVariableService _variableService = Substitute.For<IVariableService>();
    private readonly IValidationService _validationService = Substitute.For<IValidationService>();
    private readonly CommandParameterResolver _resolver;

    public CommandParameterResolverTests()
    {
        _resolver = new CommandParameterResolver(_outputService, _variableService, _validationService);
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
                new CommandParameter { Key = "param1", Validation = new Validation { Required = true }},
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
                    Validation = new Validation { Required = true }
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

    //TODO: Add test PrepareCommandParameters_ShouldCallValidate
    
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
