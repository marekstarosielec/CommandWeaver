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
                Value = new VariableValueFactory().Object().AddTextProperty("key", "testValue").Build()
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
        var result = variableResolver.ResolveVariableValue(new VariableValueFactory().Object().AddTextProperty("test", "testValue").Build());
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
                Value = new VariableValueFactory().Object().AddTextProperty("test", "testValue").Build()
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
                Value = new VariableValueFactory().Object().AddTextProperty("test", "{{ test2 }}").Build(),
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
                Value = new VariableValueFactory().List().AddElementWithTextProperty("testValue", "someProperty", "somePropertyValue").Build() //new VariableValue(new VariableValueList("key", "testValue"))
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
                Value = new VariableValueFactory().Object().AddTextProperty("value", "innerText").Build(),
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
    public void ResolveSingleValue_ResolvesElementOfList()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValueFactory().List().AddElementWithTextProperty("value1", "prop", "propValue1").AddElementWithTextProperty("value2", "prop", "propValue2").Build(),
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveVariableValue(new VariableValue("{{ test[value1] }}"));
        Assert.Null(result?.TextValue);
        Assert.Equal("value1", result?.ObjectValue?["key"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveSingleValue_ResolvesPropertyOfElementOfList()
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

    [Fact]
    public void ResolveSingleValue_ResolvesListFromAllLists()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key",new VariableValue("value1")).With("prop",new VariableValue("valueProp1"))))
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key",new VariableValue("value2")).With("prop",new VariableValue("valueProp2"))))
            } }.ToImmutableList(),
            Session = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key",new VariableValue("value3")).With("prop",new VariableValue("valueProp3"))))
            } }.ToImmutableList()
        };
        contextVariableStorage.Changes.Add(new Variable
        {
            Key = "test",
            Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key", new VariableValue("value4")).With("prop", new VariableValue("valueProp4"))))
        });
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveSingleValue("test");
        Assert.Null(result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Equal("valueProp1", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue=="value1")?["prop"].TextValue);
        Assert.Equal("valueProp2", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue == "value2")?["prop"].TextValue);
        Assert.Equal("valueProp3", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue == "value3")?["prop"].TextValue);
        Assert.Equal("valueProp4", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue == "value4")?["prop"].TextValue);
    }

    [Fact]
    public void ResolveSingleValue_OverridesListValues()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key",new VariableValue("value1")).With("prop",new VariableValue("valueProp1"))))
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key",new VariableValue("value2")).With("prop",new VariableValue("valueProp2"))))
            } }.ToImmutableList(),
            Session = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key",new VariableValue("value2")).With("prop",new VariableValue("newValue"))))
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveSingleValue("test");
        Assert.Null(result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Equal("valueProp1", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue == "value1")?["prop"].TextValue);
        Assert.Equal("newValue", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue == "value2")?["prop"].TextValue);
    }

    [Fact]
    public void ResolveSingleValue_TakesPropertyFromOverriddenListValue()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key",new VariableValue("value1")).With("prop",new VariableValue("valueProp1"))))
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key",new VariableValue("value2")).With("prop",new VariableValue("valueProp2"))))
            } }.ToImmutableList(),
            Session = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue(new VariableValueList().Add(new VariableValueObject().With("key",new VariableValue("value2")).With("prop",new VariableValue("newValue"))))
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveSingleValue("test[value2].prop");
        Assert.Equal("newValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveSingleValue_ReturnsValueFromLocal_IfSessionWasNotProvided()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue")
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue2")
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveSingleValue("test");
        Assert.Equal("testValue2", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveSingleValue_ReturnsValueFromSession_IfSessionWasProvided()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue")
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue2")
            } }.ToImmutableList(),
            Session = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue3")
            } }.ToImmutableList()
        };
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveSingleValue("test");
        Assert.Equal("testValue3", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ResolveSingleValue_ReturnsValueFromChanges_IfSessionWasProvided()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue")
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue2")
            } }.ToImmutableList(),
            Session = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValue("testValue3")
            } }.ToImmutableList()
        };
        contextVariableStorage.Changes.Add(new Variable { Key = "test", Value = new VariableValue("testValue4") });
        var variableResolver = new ContextVariableResolver(Substitute.For<IOutput>(), contextVariableStorage);
        var result = variableResolver.ResolveSingleValue("test");
        Assert.Equal("testValue4", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }
}