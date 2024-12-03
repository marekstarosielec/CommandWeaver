public class ValuePathTests
{
    [Theory]
    [InlineData("path", "{{ path }}")]
    [InlineData("  path  ", "{{   path   }}")]
    [InlineData("variableName", "{{ variableName }}")]
    public void GetWholePathAsVariable_ShouldReturnPathWithTags(string input, string expected)
    {
        var result = ValuePath.GetWholePathAsVariable(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("{{ variableName }}", "variableName")]
    [InlineData("{{ nested.variable }}", "nested.variable")]
    [InlineData("some text {{ embedded.variable }} more text", "embedded.variable")]
    [InlineData("no variable here", null)]
    [InlineData("{{ unclosed.variable", null)]
    public void ExtractVariableBetweenDelimiters_ShouldExtractVariable(string input, string? expected)
    {
        var result = ValuePath.ExtractVariableBetweenDelimiters(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("{{ variableName }}", "variableName", true)]
    [InlineData("{{ anotherVariable }}", "variableName", false)]
    [InlineData("not a variable", "variableName", false)]
    [InlineData("{{ variableName }} extra", "variableName", false)]
    public void WholePathIsSingleVariable_ShouldValidatePath(string path, string variableName, bool expected)
    {
        var result = ValuePath.WholePathIsSingleVariable(path, variableName);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("{{ variableName }}", "variableName", "value", "value")]
    [InlineData("before {{ variableName }} after", "variableName", "value", "before value after")]
    [InlineData("no match here", "variableName", "value", "no match here")]
    public void ReplaceVariableWithValue_ShouldReplaceVariableWithValue(string path, string variableName, string variableValue, string expected)
    {
        var result = ValuePath.ReplaceVariableWithValue(path, variableName, variableValue);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("variableName", true)]
    [InlineData("variable.Name", false)]
    [InlineData("variable[0]", false)]
    public void PathIsTopLevel_ShouldCheckIfPathIsTopLevel(string path, bool expected)
    {
        var result = ValuePath.PathIsTopLevel(path);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("list[0]", true)]
    [InlineData("list[index]", true)]
    [InlineData("list", false)]
    [InlineData("list.property", false)]
    public void PathIsTopLevelList_ShouldCheckIfPathIsTopLevelList(string path, bool expected)
    {
        var result = ValuePath.PathIsTopLevelList(path);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("list[0]", "0")]
    [InlineData("list[index]", "index")]
    [InlineData("notAList", null)]
    [InlineData("list.property", null)]
    public void TopLevelListKey_ShouldReturnListKey(string path, string? expected)
    {
        var result = ValuePath.TopLevelListKey(path);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetPathSections_ShouldReturnPathSections()
    {
        var path = "variableName.subProperty[0].nested[inner]";
        var result = ValuePath.GetPathSections(path);
        
        Assert.Collection(result,
            section => Assert.Equal("variableName", section.Value),
            section => Assert.Equal("subProperty", section.Value),
            section => Assert.Equal("[0]", section.Value),
            section => Assert.Equal("nested", section.Value),
            section => Assert.Equal("[inner]", section.Value));
    }

    [Theory]
    [InlineData("variableName.subProperty", "variableName")]
    [InlineData("variableName[0].subProperty", "variableName")]
    [InlineData("variableName", "variableName")]
    public void GetVariableName_ShouldReturnFirstSection(string path, string expected)
    {
        var result = ValuePath.GetVariableName(path);
        Assert.Equal(expected, result);
    }
}
