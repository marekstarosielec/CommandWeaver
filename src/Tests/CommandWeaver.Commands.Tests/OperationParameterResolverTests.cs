using NSubstitute;
using System.Collections.Immutable;

public class OperationParameterResolverTests
{
    private readonly IVariableService _variableService;
    private readonly OperationParameterResolver _resolver;
    private readonly IValidationService _validationService = Substitute.For<IValidationService>();

    public OperationParameterResolverTests()
    {
        _variableService = Substitute.For<IVariableService>();
        _resolver = new OperationParameterResolver(_variableService, _validationService);
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

    //TODO: Add test PrepareOperationParameters_ShouldCallValidate
}
