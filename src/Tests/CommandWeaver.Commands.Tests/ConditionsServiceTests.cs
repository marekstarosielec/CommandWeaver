using NSubstitute;

public class ConditionsServiceTests
{
    private readonly IOutputService _outputService;
    private readonly IFlowService _flowService;
    private readonly IVariableService _variableService;
    private readonly ConditionsService _conditionsService;

    public ConditionsServiceTests()
    {
        _outputService = Substitute.For<IOutputService>();
        _flowService = Substitute.For<IFlowService>();
        _variableService = Substitute.For<IVariableService>();
        _conditionsService = new ConditionsService(_outputService, _flowService, _variableService);
    }

    [Fact]
    public void ConditionsAreMet_ShouldReturnTrue_WhenConditionIsNull()
    {
        // Act
        var result = _conditionsService.ConditionsAreMet(null);

        // Assert
        Assert.True(result);
        _outputService.DidNotReceiveWithAnyArgs().Trace(Arg.Any<string>());
    }

    [Fact]
    public void ConditionsAreMet_ShouldReturnTrue_WhenAllConditionsAreMet()
    {
        // Arrange
        var condition = new Condition
        {
            IsNull = new DynamicValue("TestIsNull"),
            IsNotNull = new DynamicValue("TestIsNotNull")
        };

        _variableService.ReadVariableValue(condition.IsNull!).Returns(new DynamicValue());
        _variableService.ReadVariableValue(condition.IsNotNull!).Returns(new DynamicValue("NotNullValue"));

        // Act
        var result = _conditionsService.ConditionsAreMet(condition);

        // Assert
        Assert.True(result);
        _outputService.DidNotReceive().Trace(Arg.Any<string>());
    }

    [Fact]
    public void ConditionsAreMet_ShouldReturnFalse_WhenIsNullConditionFails()
    {
        // Arrange
        var condition = new Condition
        {
            IsNull = new DynamicValue("TestIsNull")
        };

        _variableService.ReadVariableValue(condition.IsNull!).Returns(new DynamicValue("NotNullValue"));

        // Act
        var result = _conditionsService.ConditionsAreMet(condition);

        // Assert
        Assert.False(result);
        _outputService.Received(1).Trace(Arg.Is<string>(msg => msg.Contains("Condition 'IsNull' not met")));
    }

    [Fact]
    public void ConditionsAreMet_ShouldReturnFalse_WhenIsNotNullConditionFails()
    {
        // Arrange
        var condition = new Condition
        {
            IsNotNull = new DynamicValue("TestIsNotNull")
        };

        _variableService.ReadVariableValue(condition.IsNotNull!).Returns(new DynamicValue());

        // Act
        var result = _conditionsService.ConditionsAreMet(condition);

        // Assert
        Assert.False(result);
        _outputService.Received(1).Trace(Arg.Is<string>(msg => msg.Contains("Condition 'IsNotNull' not met")));
    }

    [Fact]
    public void GetFromDynamicValue_ShouldReturnNull_WhenDynamicValueIsNull()
    {
        // Arrange
        var dynamicValue = new DynamicValue();

        // Act
        var result = _conditionsService.GetFromDynamicValue(dynamicValue);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetFromDynamicValue_ShouldReturnCondition_WhenDynamicValueHasValidKeys()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
        {
            { "IsNull", new DynamicValue("Value1") },
            { "IsNotNull", new DynamicValue("Value2") }
        }));

        // Act
        var result = _conditionsService.GetFromDynamicValue(dynamicValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Value1", result!.IsNull?.TextValue);
        Assert.Equal("Value2", result.IsNotNull?.TextValue);
    }

    [Fact]
    public void GetFromDynamicValue_ShouldCallFlowTerminate_WhenUnknownPropertyIsFound()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
        {
            { "IsUnknown", new DynamicValue("UnknownValue") }
        }));

        // Act & Assert
        _conditionsService.GetFromDynamicValue(dynamicValue);

        _flowService.Received(1).Terminate(Arg.Is<string>(msg => msg.Contains("Unknown condition property")));
    }
}
