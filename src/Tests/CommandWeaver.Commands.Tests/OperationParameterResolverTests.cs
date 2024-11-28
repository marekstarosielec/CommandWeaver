using NSubstitute;
using System.Collections.Immutable;

public class OperationParameterResolverTests
{
    private readonly IVariableService _variableService;
    private readonly IFlowService _flowService;
    private readonly OperationParameterResolver _resolver;

    public OperationParameterResolverTests()
    {
        _variableService = Substitute.For<IVariableService>();
        _flowService = Substitute.For<IFlowService>();
        _resolver = new OperationParameterResolver(_variableService, _flowService);
    }

    [Fact]
    public void PrepareOperationParameters_ShouldResolveValues()
    {
        // Arrange
        var operation = new TestOperation("TestOperation")
        {
            Parameters = new Dictionary<string, OperationParameter>
            {
                ["param1"] = new OperationParameter
                {
                    Description = "param1",
                    OriginalValue = new DynamicValue("{{variable1}}"),
                }
            }.ToImmutableDictionary()
        };

        var resolvedValue = new DynamicValue("resolvedValue");
        _variableService.ReadVariableValue(operation.Parameters["param1"].OriginalValue).Returns(resolvedValue);

        // Act
        var result = _resolver.PrepareOperationParameters(operation);

        // Assert
        Assert.Equal(resolvedValue, result.Parameters["param1"].Value);
        _variableService.Received(1).ReadVariableValue(operation.Parameters["param1"].OriginalValue);
    }

    [Fact]
    public void PrepareOperationParameters_ShouldTerminateIfRequiredValueIsNull()
    {
        // Arrange
        var operation = new TestOperation("TestOperation")
        {
            Parameters = new Dictionary<string, OperationParameter>
            {
                ["param1"] = new OperationParameter
                {
                    Description = "param1",
                    Required = true,
                    OriginalValue = new DynamicValue("{{variable1}}"),
                }
            }.ToImmutableDictionary()
        };

        var resolvedValue = new DynamicValue();
        _variableService.ReadVariableValue(operation.Parameters["param1"].OriginalValue).Returns(resolvedValue);

        // Act & Assert
        _resolver.PrepareOperationParameters(operation);
        _flowService.Received(1).Terminate($"Parameter 'param1' is required in operation 'TestOperation'.");
    }

    [Fact]
    public void PrepareOperationParameters_ShouldTerminateIfInvalidEnumValue()
    {
        // Arrange
        var operation = new TestOperation("TestOperation")
        {
            Parameters = new Dictionary<string, OperationParameter>
            {
                ["param1"] = new OperationParameter
                {
                    Description = "param1",
                    AllowedEnumValues = typeof(LogLevel),
                    OriginalValue = new DynamicValue("InvalidValue")
                }
            }.ToImmutableDictionary()
        };

        var resolvedValue = new DynamicValue("InvalidValue");
        _variableService.ReadVariableValue(operation.Parameters["param1"].OriginalValue).Returns(resolvedValue);

        // Act
        _resolver.PrepareOperationParameters(operation);

        // Assert
        _flowService.Received(1).Terminate($"Parameter 'param1' has an invalid value in operation 'TestOperation'.");
    }

    [Fact]
    public void PrepareOperationParameters_ShouldTerminateIfInvalidAllowedValue()
    {
        // Arrange
        var operation = new TestOperation("TestOperation")
        {
            Parameters = new Dictionary<string, OperationParameter>
            {
                ["param1"] = new OperationParameter
                {
                    Description = "param1",
                    AllowedValues = ["AllowedValue1", "AllowedValue2"],
                    OriginalValue = new DynamicValue("InvalidValue")
                }
            }.ToImmutableDictionary()
        };

        var resolvedValue = new DynamicValue("InvalidValue");
        _variableService.ReadVariableValue(operation.Parameters["param1"].OriginalValue).Returns(resolvedValue);

        // Act
        _resolver.PrepareOperationParameters(operation);

        // Assert
        _flowService.Received(1).Terminate($"Parameter 'param1' has an invalid value in operation 'TestOperation'.");
    }

    [Fact]
    public void PrepareOperationParameters_ShouldValidateTextRequirement()
    {
        // Arrange
        var operation = new TestOperation("TestOperation")
        {
            Parameters = new Dictionary<string, OperationParameter>
            {
                ["param1"] = new OperationParameter
                {
                    Description = "param1",
                    RequiredText = true,
                    OriginalValue = new DynamicValue("{{variable1}}")
                }
            }.ToImmutableDictionary()
        };

        var resolvedValue = new DynamicValue(string.Empty);
        _variableService.ReadVariableValue(operation.Parameters["param1"].OriginalValue).Returns(resolvedValue);

        // Act
        _resolver.PrepareOperationParameters(operation);

        // Assert
        _flowService.Received(1).Terminate($"Parameter 'param1' requires a text value in operation 'TestOperation'.");
    }

    [Fact]
    public void PrepareOperationParameters_ShouldValidateListRequirement()
    {
        // Arrange
        var operation = new TestOperation("TestOperation")
        {
            Parameters = new Dictionary<string, OperationParameter>
            {
                ["param1"] = new OperationParameter
                {
                    Description = "param1",
                    RequiredList = true,
                    OriginalValue = new DynamicValue("{{variable1}}")
                }
            }.ToImmutableDictionary()
        };

        var resolvedValue = new DynamicValue(new List<DynamicValue>());
        _variableService.ReadVariableValue(operation.Parameters["param1"].OriginalValue).Returns(resolvedValue);

        // Act
        _resolver.PrepareOperationParameters(operation);

        // Assert
        _flowService.DidNotReceive().Terminate(Arg.Any<string>());
    }
}
