using Models;
using Models.Interfaces.Context;
using NSubstitute;
using System.Collections.Immutable;

namespace Cli2Context.Tests;

public class ContextVariablesTests
{
    [Fact]
    public void ContextVariables_ReturnsCurrentSessionName_IfItWasDefined()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "currentSessionName",
                Value = new VariableValue("testValue")
            } }.ToImmutableList()
        };
        var contextVariables = new ContextVariables(Substitute.For<IOutput>(), contextVariableStorage);

        Assert.Equal("testValue", contextVariables.CurrentSessionName);
    }

    [Fact]
    public void ContextVariables_ReturnsDefaultCurrentSessionName_IfItWasNotDefined()
    {
        var contextVariables = new ContextVariables(Substitute.For<IOutput>());
        Assert.Equal("session1", contextVariables.CurrentSessionName);
    }

    [Fact]
    public void ContextVariables_UpdatesTextVariable_WhenItWasNotDefined()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = ImmutableList<Variable>.Empty
        };
        var contextVariables = new ContextVariables(Substitute.For<IOutput>(), contextVariableStorage);
        contextVariables.SetVariableValue(VariableScope.Command, "testVariable", new VariableValue("variableText"), "testDescription");
        Assert.Single(contextVariableStorage.Changes);
        Assert.Equal(VariableScope.Command, contextVariableStorage.Changes.First().Scope);
        Assert.Equal("testVariable", contextVariableStorage.Changes.First().Key);
        Assert.Equal("variableText", contextVariableStorage.Changes.First().Value?.TextValue);
        Assert.Equal("testDescription", contextVariableStorage.Changes.First().Description);
    }

    [Fact]
    public void ContextVariables_UpdatesObjectVariable_WhenItWasNotDefined()
    {
        var newVariableValueObject = new VariableValueObject(new Dictionary<string, VariableValue?>
        {
            { "objectProperty", new VariableValue("objectPropertyValue") }
        });
        var newVariableValue = new VariableValue { ObjectValue = newVariableValueObject };
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = ImmutableList<Variable>.Empty
        };
        var contextVariables = new ContextVariables(Substitute.For<IOutput>(), contextVariableStorage);
        contextVariables.SetVariableValue(VariableScope.Command, "testVariable", newVariableValue, "testDescription");
        Assert.Single(contextVariableStorage.Changes);
        Assert.Equal(VariableScope.Command, contextVariableStorage.Changes.First().Scope);
        Assert.Equal("testVariable", contextVariableStorage.Changes.First().Key);
        Assert.Equal("objectPropertyValue", contextVariableStorage.Changes.First().Value?.ObjectValue?["objectProperty"]?.TextValue);
        Assert.Equal("testDescription", contextVariableStorage.Changes.First().Description);
    }

    [Fact]
    public void ContextVariables_UpdatesListVariable_WhenItWasNotDefined()
    {
        var newVariableValue = new VariableValueFactory().List().AddElementWithTextProperty("firstElement", "property", "propertyValue1").AddElementWithTextProperty("secondElement", "property", "propertyValue2").Build();
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = ImmutableList<Variable>.Empty
        };
        var contextVariables = new ContextVariables(Substitute.For<IOutput>(), contextVariableStorage);
        contextVariables.SetVariableValue(VariableScope.Command, "testVariable", newVariableValue, "testDescription");
        Assert.Single(contextVariableStorage.Changes);
        Assert.Equal(VariableScope.Command, contextVariableStorage.Changes.First().Scope);
        Assert.Equal("testVariable", contextVariableStorage.Changes.First().Key);
        Assert.Equal("propertyValue1", contextVariableStorage.Changes.First().Value?.ListValue?.FirstOrDefault()?["property"]?.TextValue);
        Assert.Equal("propertyValue2", contextVariableStorage.Changes.First().Value?.ListValue?.LastOrDefault()?["property"]?.TextValue);
        Assert.Equal("testDescription", contextVariableStorage.Changes.First().Description);
    }

}
