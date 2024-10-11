using Models;

namespace Cli2Context.Tests;


public class VariableValueCrawlerTests
{

    [Fact]
    public void VariableValueCrawler_ReturnsCorrentValue_DirectlyFromVariable()
    {
        var t = new TestingSuite();
        var result = t.Sut.GetSubValue(t.Variables, "Test1");
        Assert.Equal("simple value", result);
    }

    [Fact]
    public void VariableValueCrawler_ReturnsCorrentValue_FromObjectKey()
    {
        var t = new TestingSuite();
        var result = t.Sut.GetSubValue(t.Variables, "Test2.key");
        Assert.Equal("complex value", result);
    }

    [Fact]
    public void VariableValueCrawler_ReturnsCorrentValue_FromObjectProperty()
    {
        var t = new TestingSuite();
        var result = t.Sut.GetSubValue(t.Variables, "Test2.second Property");
        Assert.Equal("secondPropertyValue", result);
    }

    [Fact]
    public void VariableValueCrawler_ReturnsWholeObject_FromList()
    {
        var t = new TestingSuite();
        var result = t.Sut.GetSubValue(t.Variables, "Test2.second Property");
        Assert.Equal("secondPropertyValue", result);
    }

    [Fact]
    public void VariableValueCrawler_ReturnsValue_FromIndex()
    {
        var t = new TestingSuite();
        var result = t.Sut.GetSubValue(t.Variables, "Test3[firstValue].key");
        Assert.Equal("firstValue", result);
    }

    [Fact]
    public void VariableValueCrawler_ReturnsSubProperty_FromIndex()
    {
        var t = new TestingSuite();
        var result = t.Sut.GetSubValue(t.Variables, "Test3[secondValue].subElement.otherProperty");
        Assert.Equal("otherPropertyValue", result);
    }

    [Fact]
    public void VariableValueCrawler_ReturnsWholeList()
    {
        var t = new TestingSuite();
        var result = t.Sut.GetSubValue(t.Variables, "Test3");
        Assert.Equal(2, (result as List<Dictionary<string, object?>>)?.Count);
    }
}


internal class TestingSuite
{
    public VariableValueCrawler Sut { get; } = new VariableValueCrawler();

    public List<Variable> Variables { get; } = new List<Variable>
    {
        new Variable
        {
            Key = "Test1",
            Value = "simple value"
        },
        new Variable
        {
            Key = "Test2",
            Value = new Dictionary<string, object?> {{ "key", "complex value"}, { "second Property", "secondPropertyValue"} }
        }
        ,
        new Variable
        {
            Key = "Test3",
            Value = new List<Dictionary<string, object?>> {
               new() { { "key", "firstValue"} },
               new() { 
                   { "key", "secondValue" } ,
                   { "subElement", new Dictionary<string, object?> { { "key", "value inside" }, { "otherProperty", "otherPropertyValue" } } 
                } }
            }
        }
    };
}