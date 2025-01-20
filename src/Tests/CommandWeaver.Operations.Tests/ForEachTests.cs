using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

public class ForEachTests
{
    private readonly ICommandService _commandService;
    private readonly IVariableService _variableService;
    private readonly IOutputService _outputService;

    public ForEachTests()
    {
        _commandService = Substitute.For<ICommandService>();
        _variableService = Substitute.For<IVariableService>();
        _outputService = Substitute.For<IOutputService>();
    }

    private ForEach CreateForEachOperation(List<DynamicValue> list, string elementName)
    {
        return new ForEach(_commandService, _variableService, _outputService)
        {
            Parameters = new Dictionary<string, OperationParameter>
            {
                {
                    "list", new OperationParameter
                    {
                        Value = new DynamicValue(list),
                        Description = "description"
                    }
                },
                {
                    "element", new OperationParameter
                    {
                        Value = new DynamicValue(elementName),
                        Description = "description"
                    }
                }
            }.ToImmutableDictionary()
        };
    }

    [Fact]
    public async Task Run_ShouldProcessEachElementInList()
    {
        // Arrange
        var listValue = new List<DynamicValue>
        {
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value1") } })),
            new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value2") } }))
        };

        var forEachOperation = CreateForEachOperation(listValue, "currentElement");

        // Act
        await forEachOperation.Run(CancellationToken.None);

        // Assert
        _variableService.Received(2).WriteVariableValue(
            VariableScope.Command,
            "currentElement",
            Arg.Any<DynamicValue>());
        await _commandService.Received(2).ExecuteOperations(
            Arg.Any<IList<Operation>>(),
            Arg.Any<CancellationToken>());
        
        _outputService.Received(2).Debug("Processing element in list");
    }

    [Fact]
    public async Task Run_ShouldSkipWhenListIsEmpty()
    {
        // Arrange
        var listValue = new List<DynamicValue>();
        var forEachOperation = CreateForEachOperation(listValue, "currentElement");

        // Act
        await forEachOperation.Run(CancellationToken.None);

        // Assert
        _variableService.DidNotReceive().WriteVariableValue(
            VariableScope.Command,
            Arg.Any<string>(),
            Arg.Any<DynamicValue>());
        await _commandService.DidNotReceive().ExecuteOperations(
            Arg.Any<IList<Operation>>(),
            Arg.Any<CancellationToken>());
        
        _outputService.DidNotReceive().Debug("Processing element in list");
    }
}
