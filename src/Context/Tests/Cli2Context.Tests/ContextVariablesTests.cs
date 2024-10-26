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

}
