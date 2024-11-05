using Models;
using Models.Interfaces.Context;
using NSubstitute;
using System.Collections.Immutable;

namespace Cli2Context.Tests;

public class ContextVariableReaderTests
{
    [Fact]
    public void ReadVariableValue_ReadTextValue_WhenTextValueContainsNoVariableTags()
    {
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), new ContextVariableStorage());
        var result = variableReader.ReadVariableValue(new DynamicValue("test"));
        Assert.Equal("test", result?.TextValue);
    }

    [Fact]
    public void ReadVariableValue_ReadTextValue_WhenTextValueContainsVariableTag()
    {
        var contextVariableStorage = new ContextVariableStorage {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("testValue")
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("{{ test }}"));
        Assert.Equal("testValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadVariableValue_ReadTextValue_WhenTextValueContainsVariableTagInsideText()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("testValue")
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("this is my {{ test }}"));
        Assert.Equal("this is my testValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadVariableValue_FailsToRead_WhenTextValueContainsNonTextVariableTagInsideText()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValueFactory().Object().AddTextProperty("key", "testValue").Build()
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("this is my {{ test }}"));
        Assert.Equal("this is my {{ test }}", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadVariableValue_ReadTextValue_WhenTextValueContainsNoVariableTags_AndWholeTextIsTreatedAsVariable()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable
            {
                Key = "test",
                Value = new DynamicValue("testValue")
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("test"), true);
        Assert.Equal("testValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadVariableValue_ReadsObjectValueFromObjectValue_WhenItDoesNotContainVariableTags()
    {
        var contextVariableStorage = new ContextVariableStorage();
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new VariableValueFactory().Object().AddTextProperty("test", "testValue").Build());
        Assert.Null(result?.TextValue);
        Assert.Equal("testValue", result?.ObjectValue?["test"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadVariableValue_ReadsObjectValueFromObjectValue_WhenItContainsVariableTags()
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
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("{{ test }}"));
        Assert.Null(result?.TextValue);
        Assert.Equal("testValue", result?.ObjectValue?["test"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadVariableValue_ReadsObjectValueFromObjectValue_WhenItContainsNestedVariableTags()
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
                Value = new DynamicValue("test2value")
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("{{ test }}"));
        Assert.Null(result?.TextValue);
        Assert.Equal("test2value", result?.ObjectValue?["test"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadVariableValue_ReadListValueFromTextValue_WhenItContainsVariableTags()
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
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("{{ test }}"));
        Assert.Null(result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Equal("testValue", result?.ListValue?.FirstOrDefault()?["key"].TextValue);
    }

    [Fact]
    public void ReadVariableValue_ReadsDeepProperty_FromObject()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValueFactory().Object().AddTextProperty("value", "innerText").Build(),
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("{{ test.value }}"));
        Assert.Equal("innerText", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadVariableValue_DoesNotThrow_WhenSelfReferencingVariableExists()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("{{ test }}")
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("{{ test }}"));
        Assert.Equal("{{ test }}", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadSingleValue_ReadsElementOfList()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new VariableValueFactory().List().AddElementWithTextProperty("value1", "prop", "propValue1").AddElementWithTextProperty("value2", "prop", "propValue2").Build(),
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("{{ test[value1] }}"));
        Assert.Null(result?.TextValue);
        Assert.Equal("value1", result?.ObjectValue?["key"]?.TextValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadSingleValue_ReadsPropertyOfElementOfList()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key",new DynamicValue("value1")).With("prop",new DynamicValue("valueProp1"))).Add(new DynamicValueObject().With("key",new DynamicValue("value2")).With("prop",new DynamicValue("valueProp2"))))
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ReadVariableValue(new DynamicValue("{{ test[value2].prop }}"));
        Assert.Equal("valueProp2", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadSingleValue_ReadsListFromAllLists()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key",new DynamicValue("value1")).With("prop",new DynamicValue("valueProp1"))))
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key",new DynamicValue("value2")).With("prop",new DynamicValue("valueProp2"))))
            } }.ToImmutableList(),
            Session = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key",new DynamicValue("value3")).With("prop",new DynamicValue("valueProp3"))))
            } }.ToImmutableList()
        };
        contextVariableStorage.Changes.Add(new Variable
        {
            Key = "test",
            Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key", new DynamicValue("value4")).With("prop", new DynamicValue("valueProp4"))))
        });
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ResolveSingleValue("test");
        Assert.Null(result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Equal("valueProp1", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue=="value1")?["prop"].TextValue);
        Assert.Equal("valueProp2", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue == "value2")?["prop"].TextValue);
        Assert.Equal("valueProp3", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue == "value3")?["prop"].TextValue);
        Assert.Equal("valueProp4", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue == "value4")?["prop"].TextValue);
    }

    [Fact]
    public void ReadSingleValue_OverridesListValues()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key",new DynamicValue("value1")).With("prop",new DynamicValue("valueProp1"))))
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key",new DynamicValue("value2")).With("prop",new DynamicValue("valueProp2"))))
            } }.ToImmutableList(),
            Session = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key",new DynamicValue("value2")).With("prop",new DynamicValue("newValue"))))
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ResolveSingleValue("test");
        Assert.Null(result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Equal("valueProp1", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue == "value1")?["prop"].TextValue);
        Assert.Equal("newValue", result?.ListValue?.FirstOrDefault(k => k["key"].TextValue == "value2")?["prop"].TextValue);
    }

    [Fact]
    public void ReadSingleValue_TakesPropertyFromOverriddenListValue()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key",new DynamicValue("value1")).With("prop",new DynamicValue("valueProp1"))))
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key",new DynamicValue("value2")).With("prop",new DynamicValue("valueProp2"))))
            } }.ToImmutableList(),
            Session = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue(new DynamicValueList().Add(new DynamicValueObject().With("key",new DynamicValue("value2")).With("prop",new DynamicValue("newValue"))))
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ResolveSingleValue("test[value2].prop");
        Assert.Equal("newValue", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadSingleValue_ReturnsValueFromLocal_IfSessionWasNotProvided()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("testValue")
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("testValue2")
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ResolveSingleValue("test");
        Assert.Equal("testValue2", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadSingleValue_ReturnsValueFromSession_IfSessionWasProvided()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("testValue")
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("testValue2")
            } }.ToImmutableList(),
            Session = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("testValue3")
            } }.ToImmutableList()
        };
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ResolveSingleValue("test");
        Assert.Equal("testValue3", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }

    [Fact]
    public void ReadSingleValue_ReturnsValueFromChanges_IfSessionWasProvided()
    {
        var contextVariableStorage = new ContextVariableStorage
        {
            BuiltIn = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("testValue")
            } }.ToImmutableList(),
            Local = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("testValue2")
            } }.ToImmutableList(),
            Session = new List<Variable> {
            new Variable {
                Key = "test",
                Value = new DynamicValue("testValue3")
            } }.ToImmutableList()
        };
        contextVariableStorage.Changes.Add(new Variable { Key = "test", Value = new DynamicValue("testValue4") });
        var variableReader = new ContextVariableReader(Substitute.For<IContext>(), contextVariableStorage);
        var result = variableReader.ResolveSingleValue("test");
        Assert.Equal("testValue4", result?.TextValue);
        Assert.Null(result?.ObjectValue);
        Assert.Null(result?.ListValue);
    }
}