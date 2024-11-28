using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

public class SetVariableTests
{
    [Fact]
    public async Task Run_ShouldWriteVariableWithSpecifiedParameters()
    {
        // Arrange
        var mockVariableService = Substitute.For<IVariableService>();
        var parameters = new Dictionary<string, OperationParameter>
        {
            { "key", new OperationParameter { Value = new DynamicValue("TestKey"), Description = "TestDescription" } },
            { "value", new OperationParameter { Value = new DynamicValue("TestValue"), Description = "TestDescription" } },
            { "scope", new OperationParameter { Value = new DynamicValue("Command"), Description = "TestDescription", AllowedEnumValues = typeof(VariableScope) } },
            { "id", new OperationParameter { Value = new DynamicValue("TestId"), Description = "TestDescription" } }
        }.ToImmutableDictionary();

        var setVariable = new SetVariable(mockVariableService) { Parameters = parameters };

        // Act
        await setVariable.Run(CancellationToken.None);

        // Assert
        mockVariableService.Received(1).WriteVariableValue(
            Arg.Is<VariableScope>(scope => scope == VariableScope.Command),
            Arg.Is<string>(key => key == "TestKey"),
            Arg.Is<DynamicValue>(value => value.TextValue == "TestValue"),
            Arg.Is<string>(id => id == "TestId")
        );
    }

    [Fact]
    public async Task Run_ShouldUseDefaultScopeWhenScopeIsNotSpecified()
    {
        // Arrange
        var mockVariableService = Substitute.For<IVariableService>();
        var parameters = new Dictionary<string, OperationParameter>
        {
            { "id", new OperationParameter { Value = new DynamicValue(), Description = "TestDescription" } },
            { "key", new OperationParameter { Value = new DynamicValue("TestKey"), Description = "TestDescription" } },
            { "scope", new OperationParameter { Value = new DynamicValue(), Description = "TestDescription" } },
            { "value", new OperationParameter { Value = new DynamicValue("TestValue"), Description = "TestDescription" } }
        }.ToImmutableDictionary();

        var setVariable = new SetVariable(mockVariableService) { Parameters = parameters };

        // Act
        await setVariable.Run(CancellationToken.None);

        // Assert
        mockVariableService.Received(1).WriteVariableValue(
            Arg.Is<VariableScope>(scope => scope == VariableScope.Command),
            Arg.Is<string>(key => key == "TestKey"),
            Arg.Is<DynamicValue>(value => value.TextValue == "TestValue"),
            Arg.Is<string?>(id => id == null)
        );
    }
}
