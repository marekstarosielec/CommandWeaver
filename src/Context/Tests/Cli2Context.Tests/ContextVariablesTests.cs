// using Models;
//
using Newtonsoft.Json.Linq;

namespace Cli2Context.Tests;
//
public class ContextVariablesTests
{
    [Fact]
    public void GetValueAsString_ReturnsStringWithoutAnyEvaluation_IfKeyDoesNotContainVariables()
    {
        var variables = new ContextVariables();
        var result = variables.GetValueAsString("test");
        Assert.Equal("test", result);
    }

    [Fact]
    public void GetValueAsString_ReturnsStringWithEvaluation_IfKeyContainsVariables()
    {
        var variables = new ContextVariables();
        variables.SetVariableList(Models.RepositoryLocation.BuiltIn, [new() { Key = "abc", Value = "value" }]);
        var result = variables.GetValueAsString("test{{ abc }}");
        Assert.Equal("testvalue", result);
    }

    [Fact]
    public void GetValueAsString_ReturnsStringWithEvaluation_IfKeyIsVariable()
    {
        var variables = new ContextVariables();
        variables.SetVariableList(Models.RepositoryLocation.BuiltIn, [new() { Key = "abc", Value = "value" }]);
        var result = variables.GetValueAsString("{{ abc }}");
        Assert.Equal("value", result);
    }

    [Fact]
    public void GetValueAsString_ReturnsStringWithEvaluation_IfKeyContainsNestedVariables()
    {
        var variables = new ContextVariables();
        variables.SetVariableList(Models.RepositoryLocation.BuiltIn, [new() { Key = "abcnested", Value = "value" }, new() { Key = "123", Value = "nested" }]);
        var result = variables.GetValueAsString("test{{ abc{{ 123 }} }}");
        Assert.Equal("testvalue", result);
    }


    [Fact]
    public void GetValueAsString_ReturnsStringWithEvaluation_IfKeyContainsMultipleNestedVariables()
    {
        var variables = new ContextVariables();
        variables.SetVariableList(Models.RepositoryLocation.BuiltIn, [new() { Key = "abcnested", Value = "value" }, new() { Key = "123", Value = "nested" }, new() { Key = "def", Value = "defValue" }]);
        var result = variables.GetValueAsString("test{{ abc{{ 123 }} }}div{{ def}}{{ def}}");
        Assert.Equal("testvaluedivdefValuedefValue", result);
    }

    //     [Fact]
    //     public void ContextVariables_ReturnsValueFromBuiltIn_IfOthersWereNotProvided()
    //     {
    //         var sut = new ContextVariables();
    //         sut._builtIn.Add(new Variable { Key = "key", Value = "value"});
    //         Assert.Equal("value", sut.GetVariable("key")?.Value as string);
    //     }
    //     
    //     [Fact]
    //     public void ContextVariables_ReturnsValueFromLocal_IfSessionWasNotProvided()
    //     {
    //         var sut = new ContextVariables();
    //         sut._builtIn.Add(new Variable { Key = "key", Value = "value"});
    //         sut._local.Add(new Variable { Key = "key", Value = "value2"});
    //
    //         Assert.Equal("value2", sut.GetVariable("key")?.Value as string);
    //     }
    //     
    //     [Fact]
    //     public void ContextVariables_ReturnsValueFromSession_IfSessionWasProvided()
    //     {
    //         var sut = new ContextVariables();
    //         sut._builtIn.Add(new Variable { Key = "key", Value = "value"});
    //         sut._local.Add(new Variable { Key = "key", Value = "value2"});
    //         sut._session.Add(new Variable { Key = "key", Value = "value3"});
    //
    //         Assert.Equal("value3", sut.GetVariable("key")?.Value as string);
    //     }
    //     
    //     [Fact]
    //     public void ContextVariables_ReturnsCurrentSessionName_IfItWasDefined()
    //     {
    //         var sut = new ContextVariables();
    //         sut._builtIn.Add(new Variable { Key = "currentSessionName", Value = "value"});
    //         Assert.Equal("value", sut.CurrentSessionName);
    //     }
    //     
    //     [Fact]
    //     public void ContextVariables_ReturnsDefaultCurrentSessionName_IfItWasNotDefined()
    //     {
    //         var sut = new ContextVariables();
    //         Assert.Equal("session1", sut.CurrentSessionName);
    //     }
}