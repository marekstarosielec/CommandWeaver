using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NSubstitute;

public class WriterTests
{
    private readonly IFlowService _flowService = Substitute.For<IFlowService>();
    private readonly IVariableStorage _variableStorage = Substitute.For<IVariableStorage>();
    private readonly IRepository _repository = Substitute.For<IRepository>();
    private readonly Writer _writer;

    public WriterTests()
    {
        _writer = new Writer(_flowService, _variableStorage, _repository);
    }

    private void SetupRepositoryPath(string basePath)
    {
        _repository.GetPath(RepositoryLocation.Session, Arg.Any<string?>()).Returns(basePath);
        _repository.GetPath(RepositoryLocation.Application, Arg.Any<string?>()).Returns(basePath);
    }

    [Fact]
    public void WriteVariableValue_ShouldWriteTopLevelVariable()
    {
        // Arrange
        var scope = VariableScope.Session;
        var sessionName = "testSession";
        var path = "variable1";
        var value = new DynamicValue("value");
        var repositoryElementId = "repository1";

        SetupRepositoryPath("basePath");

        // Act
        _writer.WriteVariableValue(scope, sessionName, path, value, repositoryElementId);

        // Assert
        _repository.Received(1).GetPath(RepositoryLocation.Session, sessionName);
        _variableStorage.Received(1).RemoveAllInScope(VariableScope.Command, Arg.Any<Predicate<Variable>>());
        _variableStorage.Received(1).RemoveAllInScope(VariableScope.Session, Arg.Any<Predicate<Variable>>());
        _variableStorage.Received(1).Add(VariableScope.Session, Arg.Is<Variable>(v =>
            v.Key == path &&
            v.Value == value));
    }

    [Fact]
    public void WriteVariableValue_ShouldWriteTopLevelList()
    {
        // Arrange
        var scope = VariableScope.Application;
        var path = "list[0]";
        var key = "0";
        var value = new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
        {
            { "key", new DynamicValue(key) },
            { "value", new DynamicValue("listValue") }
        }));

        SetupRepositoryPath("basePath");

        // Act
        _writer.WriteVariableValue(scope, null, path, value, null);

        // Assert
        _variableStorage.Received(1).RemoveAllBelowScope(VariableScope.Application, Arg.Any<Predicate<Variable>>());
        _variableStorage.Received(1).Add(VariableScope.Application, Arg.Is<Variable>(v =>
            v.Key == "list" &&
            v.Value.ListValue!.Any(e => e.ObjectValue!["key"].TextValue == key)));
    }

    [Fact]
    public void WriteVariableValue_ShouldThrowWhenWritingToSubProperty()
    {
        // Arrange
        var scope = VariableScope.Command;
        var path = "variable.property";
        var value = new DynamicValue("value");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _writer.WriteVariableValue(scope, null, path, value, null));
        Assert.Equal("Writing to sub-property is not supported.", exception.Message);
        _flowService.Received(1).Terminate("Writing to sub-property is not supported.");
    }

    [Fact]
    public void WriteVariableValue_ShouldThrowWhenRepositoryElementIdMissingForNonCommandScope()
    {
        // Arrange
        var scope = VariableScope.Session;
        var sessionName = "testSession";
        var path = "variable";
        var value = new DynamicValue("value");

        SetupRepositoryPath("basePath");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _writer.WriteVariableValue(scope, sessionName, path, value, null));
        Assert.Equal("Repository element ID must be specified for non-command scopes.", exception.Message);
        _flowService.Received(1).Terminate("Repository element ID must be specified for non-command scopes.");
    }

    [Fact]
    public void WriteVariableValue_ShouldUpdateExistingListElement()
    {
        // Arrange
        var scope = VariableScope.Command;
        var path = "list[0]";
        var key = "0";
        var existingVariable = new Variable
        {
            Key = "list",
            Value = new DynamicValue(new DynamicValueList(new[]
            {
                new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
                {
                    { "key", new DynamicValue(key) },
                    { "value", new DynamicValue("oldValue") }
                }))
            }))
        };

        var value = new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
        {
            { "key", new DynamicValue(key) },
            { "value", new DynamicValue("newValue") }
        }));

        _variableStorage.FirstOrDefault(scope, Arg.Any<Func<Variable, bool>>()).Returns(existingVariable);

        // Act
        _writer.WriteVariableValue(scope, null, path, value, null);

        // Assert
        Assert.NotNull(existingVariable.Value.ListValue);
        Assert.Equal("newValue", existingVariable.Value.ListValue!.First().ObjectValue!["value"].TextValue);
    }

    [Fact]
    public void WriteVariableValue_ShouldAddNewListElement()
    {
        // Arrange
        var scope = VariableScope.Application;
        var path = "list[1]";
        var key = "1";
        var value = new DynamicValue(new DynamicValueObject(new Dictionary<string, DynamicValue?>
        {
            { "key", new DynamicValue(key) },
            { "value", new DynamicValue("listValue") }
        }));

        SetupRepositoryPath("basePath");

        // Act
        _writer.WriteVariableValue(scope, null, path, value, null);

        // Assert
        _variableStorage.Received(1).Add(VariableScope.Application, Arg.Is<Variable>(v =>
            v.Key == "list" &&
            v.Value.ListValue!.Any(e => e.ObjectValue!["key"].TextValue == key)));
    }
}
