using System.Collections.Immutable;

namespace CommandLine.Tests;

public class ParserTests
{
    [Fact]
    public void Test_Flags_Only()
    {
        // Arrange
        const string input = "--flag1 -flag2";

        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("flag1", result[0].Name);
        Assert.Equal("true", result[0].Value);
        Assert.Equal("argument", result[0].Type);
        
        Assert.Equal("flag2", result[1].Name);
        Assert.Equal("true", result[1].Value);
        Assert.Equal("argument", result[1].Type);
    }

    [Fact]
    public void Test_Command_With_No_Flag_Or_Arguments()
    {
        // Arrange
        // Arrange
        const string input = "command1";

        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();
    
        // Assert
        Assert.Single(result);
        Assert.Equal("command1", result[0].Name);
        Assert.Null(result[0].Value);
        Assert.Equal("command", result[0].Type);
    }
    
    [Fact]
    public void Test_Arguments_With_Single_Quoted_Value()
    {
        // Arrange
        const string input = "--arg1 'single quoted value'";
    
        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();

        // Assert
        Assert.Single(result);
        Assert.Equal("arg1", result[0].Name);
        Assert.Equal("single quoted value", result[0].Value);
        Assert.Equal("argument", result[0].Type);
    }
    
    [Fact]
    public void Test_Arguments_With_Double_Quoted_Value()
    {
        // Arrange
        const string input = "--arg1 \"double quoted value\"";
    
        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();

        // Assert
        Assert.Single(result);
        Assert.Equal("arg1", result[0].Name);
        Assert.Equal("double quoted value", result[0].Value);
        Assert.Equal("argument", result[0].Type);
    }
    
    [Fact]
    public void Test_Escaped_Quotes_Inside_Value()
    {
        // Arrange
        const string input = "--arg1 \"value with \\\"escaped\\\" quotes\"";
    
        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();

        // Assert
        Assert.Single(result);
        Assert.Equal("arg1", result[0].Name);
        Assert.Equal("value with \"escaped\" quotes", result[0].Value);
        Assert.Equal("argument", result[0].Type);
    }
    
    [Fact]
    public void Test_Command_With_Flag_And_Argument()
    {
        // Arrange
        const string input = "command1 --flag1 --arg1 \"value with spaces\"";
    
        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();

        // Assert
        Assert.Equal(3, result.Count);
    
        Assert.Equal("command1", result[0].Name);
        Assert.Null(result[0].Value);
        Assert.Equal("command", result[0].Type);
    
        Assert.Equal("flag1", result[1].Name);
        Assert.Equal("true", result[1].Value);
        Assert.Equal("argument", result[1].Type);
    
        Assert.Equal("arg1", result[2].Name);
        Assert.Equal("value with spaces", result[2].Value);
        Assert.Equal("argument", result[2].Type);
    }

    
    [Fact]
    public void Test_Multiple_Arguments_With_Quotes_And_Escapes()
    {
        // Arrange
        const string input = "--arg1 'first value' --arg2 \"second value with spaces\" --arg3 \"escaped \\\"value\\\"\"";
    
        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();

        // Assert
        Assert.Equal(3, result.Count);
    
        Assert.Equal("arg1", result[0].Name);
        Assert.Equal("first value", result[0].Value);
        Assert.Equal("argument", result[0].Type);
    
        Assert.Equal("arg2", result[1].Name);
        Assert.Equal("second value with spaces", result[1].Value);
        Assert.Equal("argument", result[1].Type);
    
        Assert.Equal("arg3", result[2].Name);
        Assert.Equal("escaped \"value\"", result[2].Value);
        Assert.Equal("argument", result[2].Type);
    }
    
    [Fact]
    public void Test_Empty_Input_Should_Return_Empty_List()
    {
        // Arrange
        const string input = "";
    
        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();

        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void Test_Input_With_Only_Whitespace_Should_Return_Empty_List()
    {
        // Arrange
        const string input = "   ";
    
        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();

        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void Test_Invalid_Argument_Format()
    {
        // Arrange
        const string input = "--arg1valuewithoutspace";
    
        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();

        // Assert
        // No argument is recognized because the space is missing between the argument name and value
        Assert.Single(result);
        Assert.Equal("arg1valuewithoutspace", result[0].Name);
        Assert.Equal("true", result[0].Value);
        Assert.Equal("argument", result[0].Type);
    }
    
    [Fact]
    public void Test_Argument_With_Double_Quotes_Inside_Single_Quotes()
    {
        // Arrange
        const string input = "--arg1 'value with \"double quotes\" inside'";
    
        // Act
        var result = new Parser().ParseArguments(input).ToImmutableList();

        // Assert
        Assert.Single(result);
        Assert.Equal("arg1", result[0].Name);
        Assert.Equal("value with \"double quotes\" inside", result[0].Value);
        Assert.Equal("argument", result[0].Type);
    }
}