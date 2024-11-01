namespace Cli2Context.Tests;

public class VariableValuePathTests
{
    [Fact]
    public void GetWholePathAsVariable_ReturnsVariableInTags() => Assert.Equal("{{ test }}", VariableValuePath.GetWholePathAsVariable("test"));

    [Fact]
    public void ExtractVariableBetweenDelimiters_ReturnsNullWhenNoVariableWasPresent() => Assert.Null(VariableValuePath.ExtractVariableBetweenDelimiters("test"));

    [Fact]
    public void ExtractVariableBetweenDelimiters_ReturnsValueWhenVariableWasPresent() => Assert.Equal("test", VariableValuePath.ExtractVariableBetweenDelimiters("{{ test}}"));

    [Fact]
    public void ExtractVariableBetweenDelimiters_ReturnsFirstValueWhenMultipleVariablesWerePresent() => Assert.Equal("test", VariableValuePath.ExtractVariableBetweenDelimiters("{{ test }} test2 {{test3}}"));

    [Fact]
    public void ExtractVariableBetweenDelimiters_ReturnsDeepestValueWhenDeepVariablesWerePresent() => Assert.Equal("test3", VariableValuePath.ExtractVariableBetweenDelimiters("{{ test {{test3}}}}"));

    [Fact]
    public void WholePathIsSingleVariable_ReturnsTrue_WhenWholePathIsSingleVariable() => Assert.True(VariableValuePath.WholePathIsSingleVariable("{{ test }}", "test"));

    [Fact]
    public void WholePathIsSingleVariable_ReturnsFals_WhenWholePathIsNotSingleVariable() => Assert.False(VariableValuePath.WholePathIsSingleVariable("{{ test }} more text", "test"));

    [Fact]
    public void ReplaceVariableWithValue_ReplacesVariableWithValue() => Assert.Equal("some nontest value", VariableValuePath.ReplaceVariableWithValue("some {{ test }} value", "test", "nontest"));

    [Fact]
    public void ReplaceVariableWithValue_ReplacesVariableWithValue_InNestedVariable() => Assert.Equal("some {{ testnontest }} value", VariableValuePath.ReplaceVariableWithValue("some {{ test{{test2}} }} value", "test2", "nontest"));

    [Fact]
    public void ReplaceVariableWithValue_ReplacesVariableWithValue_WhenMultipleSameVariableWereUsed() => Assert.Equal("some nontest value nontest", VariableValuePath.ReplaceVariableWithValue("some {{ test }} value {{test}}", "test", "nontest"));

    [Fact]
    public void PathIsTopLevel_ReturnsTrue_IfPathPointsToTopVariable() => Assert.True(VariableValuePath.PathIsTopLevel("test"));

    [Fact]
    public void PathIsTopLevel_ReturnsFalse_IfPathPointsToSubObject() => Assert.False(VariableValuePath.PathIsTopLevel("test.prop"));

    [Fact]
    public void PathIsTopLevel_ReturnsFalse_IfPathPointsToListElement() => Assert.False(VariableValuePath.PathIsTopLevel("test[prop]"));

    [Fact]
    public void PathIsTopLevel_ReturnsFalse_IfPathPointsToSubObjectInListElement() => Assert.False(VariableValuePath.PathIsTopLevel("test[prop].sub"));

    [Fact]
    public void GetPathSections_ReturnsCorrectResult_ForTopVariable()
    {
        var result = VariableValuePath.GetPathSections("test");
        Assert.Single(result);
        Assert.Equal("test", result[0].Groups[1].Value);
        Assert.True(result[0].Groups[1].Success);
    }

    [Fact]
    public void GetPathSections_ReturnsCorrectResult_ForListIndex()
    {
        var result = VariableValuePath.GetPathSections("test[test2]");
        Assert.Equal(2, result.Count);
        Assert.Equal("test", result[0].Groups[1].Value);
        Assert.True(result[0].Groups[1].Success);
        Assert.Equal("test2", result[1].Groups[2].Value);
        Assert.True(result[1].Groups[2].Success);
    }

    [Fact]
    public void GetPathSections_ReturnsCorrectResult_ForSubProperty()
    {
        var result = VariableValuePath.GetPathSections("test.test2");
        Assert.Equal(2, result.Count);
        Assert.Equal("test", result[0].Groups[1].Value);
        Assert.True(result[0].Groups[1].Success);
        Assert.Equal("test2", result[1].Groups[1].Value);
        Assert.True(result[1].Groups[1].Success);
    }

    [Fact]
    public void GetPathSections_ReturnsCorrectResult_ForSubPropertyInList()
    {
        var result = VariableValuePath.GetPathSections("test[test2].test3");
        Assert.Equal(3, result.Count);
        Assert.Equal("test", result[0].Groups[1].Value);
        Assert.True(result[0].Groups[1].Success);
        Assert.Equal("test2", result[1].Groups[2].Value);
        Assert.True(result[1].Groups[2].Success);
        Assert.Equal("test3", result[2].Groups[1].Value);
        Assert.True(result[2].Groups[1].Success);
    }

    [Fact]
    public void GetVariableName_ReturnsCorrectResult_ForSingleVariable() => Assert.Equal("test", VariableValuePath.GetVariableName("test"));
    
    [Fact]
    public void GetVariableName_ReturnsCorrectResult_ForSubPropertyVariable() => Assert.Equal("test", VariableValuePath.GetVariableName("test.test2"));

    [Fact]
    public void GetVariableName_ReturnsCorrectResult_ForListVariable() => Assert.Equal("test", VariableValuePath.GetVariableName("test[test2]"));

    [Fact]
    public void GetVariableName_ReturnsCorrectResult_ForSubPropertyInListVariable() => Assert.Equal("test", VariableValuePath.GetVariableName("test[test2].test3"));

}
