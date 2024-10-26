using Models;
using Models.Interfaces.Context;
using NSubstitute;
using System.Collections.Immutable;

namespace Cli2Context.Tests;

public class ContextVariableResolverTests
{
    [Fact]
    public void ResolveVariableValue_ResolvesTextValue_WhenTextValueContainsNoVariableTags()
    {
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), new ContextVariableStorage());
        var result = variableResolver.ResolveVariableValue(new VariableValue("test"));
        Assert.Equal("test", result?.TextValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesTextValue_WhenTextValueContainsVariableTag()
    {
        var contextVariableStorage = new ContextVariableStorage {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue")
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("{{ test }}"));
        Assert.Equal("testValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesTextValue_WhenTextValueContainsVariableTagInsideText()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue")
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("this is my {{ test }}"));
        Assert.Equal("this is my testValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_FailsToResolve_WhenTextValueContainsNonTextVariableTagInsideText()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueObject("key", "testValue"))
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("this is my {{ test }}"));
        Assert.Equal("this is my {{ test }}", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesTextValue_WhenTextValueContainsNoVariableTags_AndWholeTextIsTreatedAsVariable()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable
            {
                Key = "test",
                Value = new VariableValue("testValue")
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("test"), true);
        Assert.Equal("testValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesObjectValueFromObjectValue_WhenItDoesNotContainVariableTags()
    {
        var contextVariableStorage = new ContextVariableStorage();
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue(new VariableValueObject("test", "testValue")));
        Assert.Null(result?.TextValue);
        Assert.Equal("testValue", result?.ObjectValue?["test"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesObjectValueFromObjectValue_WhenItContainsVariableTags()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable
            {
                Key = "test",
                Value = new VariableValue(new VariableValueObject("test", "testValue"))
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("{{ test }}"));
        Assert.Null(result?.TextValue);
        Assert.Equal("testValue", result?.ObjectValue?["test"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesObjectValueFromObjectValue_WhenItContainsNestedVariableTags()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable
            {
                Key = "test",
                Value = new VariableValue(new VariableValueObject("test", "{{ test2 }}"))
            },
            new Variable
            {
                Key = "test2",
                Value = new VariableValue("test2value")
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("{{ test }}"));
        Assert.Null(result?.TextValue);
        Assert.Equal("test2value", result?.ObjectValue?["test"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesListValueFromTextValue_WhenItContainsVariableTags()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable
            {
                Key = "test",
                Value = new VariableValue(new VariableValueList("key", "testValue"))
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("{{ test }}"));
        Assert.Null(result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Equal("testValue", result?.ListValue?.FirstOrDefault()?["key"].TextValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesDeepProperty_FromObject()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueObject("key", "innerObject").With("value", new VariableValue("innerText")))
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("{{ test.value }}"));
        Assert.Equal("innerText", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_DoesNotThrow_WhenSelfReferencinVariableExists()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("{{ test }}")
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("{{ test }}"));
        Assert.Equal("{{ test }}", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesElementOfList()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject("key","value1")).Add(new VariableValueObject("key","value2")))
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("{{ test[value1] }}"));
        Assert.Null(result?.TextValue);
        Assert.Equal("value1", result?.ObjectValue?["key"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveVariableValue_ResolvesPropertyOfElementOfList()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key",new VariableValue("value1")).With("prop",new VariableValue("valueProp1"))).Add(new VariableValueObject().With("key",new VariableValue("value2")).With("prop",new VariableValue("valueProp2"))))
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("{{ test[value2].prop }}"));
        Assert.Equal("valueProp2", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }


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