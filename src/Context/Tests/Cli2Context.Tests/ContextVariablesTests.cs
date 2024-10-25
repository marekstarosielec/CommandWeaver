using Models;
using Models.Interfaces.Context;
using NSubstitute;

namespace Cli2Context.Tests;

public class ContextVariablesTests
{
    [Fact]
    public void ResolveVariableValue_ResolvesTextValue_WhenTextValueContainsNoVariableTags()
    {
        var variables = new ContextVariables(Substitute.For<IOutput>());
        var result = variables.ResolveVariableValue(new VariableValue("test"));
        Assert.Equal("test", result?.TextValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesTextValue_WhenTextValueContainsVariableTag()
    {
        var variables = new ContextVariables(Substitute.For<IOutput>());
        variables.SetVariableList(RepositoryLocation.BuiltIn, new List<Variable?> { 
            new Variable { 
                Key = "test", 
                Value = new VariableValue("testValue")
            } 
        });
        var result = variables.ResolveVariableValue(new VariableValue("{{ test }}"));
        Assert.Equal("testValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesTextValue_WhenTextValueContainsVariableTagInsideText()
    {
        var variables = new ContextVariables(Substitute.For<IOutput>());
        variables.SetVariableList(RepositoryLocation.BuiltIn, new List<Variable?> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue")
            }
        });
        var result = variables.ResolveVariableValue(new VariableValue("this is my {{ test }}"));
        Assert.Equal("this is my testValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_FailsToResolve_WhenTextValueContainsNonTextVariableTagInsideText()
    {
        var variables = new ContextVariables(Substitute.For<IOutput>());
        variables.SetVariableList(RepositoryLocation.BuiltIn, new List<Variable?> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueObject("key", "testValue"))
            }
        });
        var result = variables.ResolveVariableValue(new VariableValue("this is my {{ test }}"));
        Assert.Equal("this is my {{ test }}", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesTextValue_WhenTextValueContainsNoVariableTags_AndWholeTextIsTreatedAsVariable()
    {
        var variables = new ContextVariables(Substitute.For<IOutput>());
        variables.SetVariableList(RepositoryLocation.BuiltIn, new List<Variable?> 
        { 
            new Variable 
            { 
                Key = "test", 
                Value = new VariableValue("testValue")
            } 
        });
        var result = variables.ResolveVariableValue(new VariableValue("test"), true);
        Assert.Equal("testValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesObjectValueFromObjectValue_WhenItDoesNotContainVariableTags()
    {
        var variables = new ContextVariables(Substitute.For<IOutput>());
        var result = variables.ResolveVariableValue(
            new VariableValue(new VariableValueObject("test", "testValue"))
        );
        Assert.Null(result?.TextValue);
        Assert.Equal("testValue", result?.ObjectValue?["test"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesObjectValueFromObjectValue_WhenItContainsVariableTags()
    {
        var variables = new ContextVariables(Substitute.For<IOutput>());
        variables.SetVariableList(RepositoryLocation.BuiltIn, new List<Variable?> 
        { 
            new Variable 
            { 
                Key = "test", 
                Value = new VariableValue(new VariableValueObject("test", "testValue"))
            } 
        });
        var result = variables.ResolveVariableValue(new VariableValue("{{ test }}"));
        Assert.Null(result?.TextValue);
        Assert.Equal("testValue", result?.ObjectValue?["test"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesObjectValueFromObjectValue_WhenItContainsNestedVariableTags()
    {
        var variables = new ContextVariables(Substitute.For<IOutput>());
        variables.SetVariableList(RepositoryLocation.BuiltIn, new List<Variable?>
        {
            new Variable
            {
                Key = "test",
                Value = new VariableValue(new VariableValueObject("test", "{{ test2 }}"))
            },
            new Variable
            {
                Key = "test2",
                Value = new VariableValue("test2value")
            }
        });
        var result = variables.ResolveVariableValue(new VariableValue("{{ test }}"));
        Assert.Null(result?.TextValue);
        Assert.Equal("test2value", result?.ObjectValue?["test"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesListValueFromTextValue_WhenItContainsVariableTags()
    {
        var variables = new ContextVariables(Substitute.For<IOutput>());
        variables.SetVariableList(RepositoryLocation.BuiltIn, new List<Variable?>
        {
            new Variable
            {
                Key = "test",
                Value = new VariableValue(new VariableValueList("key", "testValue"))
            }
        });
        var result = variables.ResolveVariableValue(new VariableValue("{{ test }}"));
        Assert.Null(result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Equal("testValue", result?.ListValue?.FirstOrDefault()?["key"].TextValue);
    }

    //    [Fact]
    //    public void GetValueAsString_ReturnsStringWithoutAnyEvaluation_IfKeyDoesNotContainVariables()
    //    {
    //        var variables = new ContextVariables();
    //        var result = variables.GetValueAsString("test");
    //        Assert.Equal("test", result);
    //    }

    //    [Fact]
    //    public void GetValueAsString_ReturnsStringWithEvaluation_IfKeyContainsVariables()
    //    {
    //        var variables = new ContextVariables();
    //        variables.SetVariableList(Models.RepositoryLocation.BuiltIn, [new() { Key = "abc", Value = "value" }]);
    //        var result = variables.GetValueAsString("test{{ abc }}");
    //        Assert.Equal("testvalue", result);
    //    }

    //    [Fact]
    //    public void GetValueAsString_ReturnsStringWithEvaluation_IfKeyIsVariable()
    //    {
    //        var variables = new ContextVariables();
    //        variables.SetVariableList(Models.RepositoryLocation.BuiltIn, [new() { Key = "abc", Value = "value" }]);
    //        var result = variables.GetValueAsString("{{ abc }}");
    //        Assert.Equal("value", result);
    //    }

    //    [Fact]
    //    public void GetValueAsString_ReturnsStringWithEvaluation_IfKeyContainsNestedVariables()
    //    {
    //        var variables = new ContextVariables();
    //        variables.SetVariableList(Models.RepositoryLocation.BuiltIn, [new() { Key = "abcnested", Value = "value" }, new() { Key = "123", Value = "nested" }]);
    //        var result = variables.GetValueAsString("test{{ abc{{ 123 }} }}");
    //        Assert.Equal("testvalue", result);
    //    }


    //    [Fact]
    //    public void GetValueAsString_ReturnsStringWithEvaluation_IfKeyContainsMultipleNestedVariables()
    //    {
    //        var variables = new ContextVariables();
    //        variables.SetVariableList(Models.RepositoryLocation.BuiltIn, [new() { Key = "abcnested", Value = "value" }, new() { Key = "123", Value = "nested" }, new() { Key = "def", Value = "defValue" }]);
    //        var result = variables.GetValueAsString("test{{ abc{{ 123 }} }}div{{ def}}{{ def}}");
    //        Assert.Equal("testvaluedivdefValuedefValue", result);
    //    }

    //    [Fact]
    //    public void GetValueAsString_ReturnsStringWithEvaluation_IfValueContainsAnotherVariable()
    //    {
    //        var variables = new ContextVariables();
    //        variables.SetVariableList(Models.RepositoryLocation.BuiltIn, [new() { Key = "123", Value = "{{abc}}" }, new() { Key = "abc", Value = "result" }]);
    //        var result = variables.GetValueAsString("test{{123}}");
    //        Assert.Equal("testresult", result);
    //    }

    //    //     [Fact]
    //    //     public void ContextVariables_ReturnsValueFromBuiltIn_IfOthersWereNotProvided()
    //    //     {
    //    //         var sut = new ContextVariables();
    //    //         sut._builtIn.Add(new Variable { Key = "key", Value = "value"});
    //    //         Assert.Equal("value", sut.GetVariable("key")?.Value as string);
    //    //     }
    //    //     
    //    //     [Fact]
    //    //     public void ContextVariables_ReturnsValueFromLocal_IfSessionWasNotProvided()
    //    //     {
    //    //         var sut = new ContextVariables();
    //    //         sut._builtIn.Add(new Variable { Key = "key", Value = "value"});
    //    //         sut._local.Add(new Variable { Key = "key", Value = "value2"});
    //    //
    //    //         Assert.Equal("value2", sut.GetVariable("key")?.Value as string);
    //    //     }
    //    //     
    //    //     [Fact]
    //    //     public void ContextVariables_ReturnsValueFromSession_IfSessionWasProvided()
    //    //     {
    //    //         var sut = new ContextVariables();
    //    //         sut._builtIn.Add(new Variable { Key = "key", Value = "value"});
    //    //         sut._local.Add(new Variable { Key = "key", Value = "value2"});
    //    //         sut._session.Add(new Variable { Key = "key", Value = "value3"});
    //    //
    //    //         Assert.Equal("value3", sut.GetVariable("key")?.Value as string);
    //    //     }
    //    //     
    //    //     [Fact]
    //    //     public void ContextVariables_ReturnsCurrentSessionName_IfItWasDefined()
    //    //     {
    //    //         var sut = new ContextVariables();
    //    //         sut._builtIn.Add(new Variable { Key = "currentSessionName", Value = "value"});
    //    //         Assert.Equal("value", sut.CurrentSessionName);
    //    //     }
    //    //     
    //    //     [Fact]
    //    //     public void ContextVariables_ReturnsDefaultCurrentSessionName_IfItWasNotDefined()
    //    //     {
    //    //         var sut = new ContextVariables();
    //    //         Assert.Equal("session1", sut.CurrentSessionName);
    //    //     }
}