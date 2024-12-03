using Spectre.Console;

public class MarkupConverterTests
{
    [Fact]
    public void ConvertToSegments_ShouldApplyStylesCorrectly()
    {
        // Arrange
        var input = "Hello [[#FF0000,b]]World[[/]]!";

        // Act
        var segments = MarkupConverter.ConvertToSegments(input).ToList();

        // Assert
        Assert.Equal(3, segments.Count);
        Assert.Equal("Hello ", segments[0].Text);
        Assert.Equal("World", segments[1].Text);
        Assert.Equal(Color.Red, segments[1].Style.Foreground);
        Assert.Equal(Decoration.Bold, segments[1].Style.Decoration);
        Assert.Equal("!", segments[2].Text);
    }

    [Fact]
    public void ConvertToSegments_ShouldHandleEscapedBrackets()
    {
        // Arrange
        var input = "Value /[/[escaped/]/] brackets";

        // Act
        var segments = MarkupConverter.ConvertToSegments(input).ToList();

        // Assert
        Assert.Single(segments);
        Assert.Equal("Value [[escaped]] brackets", segments[0].Text);
    }

    [Fact]
    public void ConvertToSegments_ShouldHandleEmptyInput()
    {
        // Arrange
        var input = "";

        // Act
        var segments = MarkupConverter.ConvertToSegments(input).ToList();

        // Assert
        Assert.Empty(segments);
    }

    [Theory]
    [InlineData("[[style]]Text", 1)]
    [InlineData("No styles here", 1)]
    [InlineData("[[style]]Text[[/]]", 1)]
    [InlineData("[[style]]Text[[/]]abc", 2)]
    [InlineData("[[style]]Text[[/]]abc[[style]]123", 3)]
    public void ConvertToSegments_ShouldSplitInputCorrectly(string input, int expectedSegmentCount)
    {
        // Act
        var segments = MarkupConverter.ConvertToSegments(input).ToList();

        // Assert
        Assert.Equal(expectedSegmentCount, segments.Count);
    }

    [Theory]
    [InlineData("[[b]]", null, Decoration.Bold)]
    [InlineData("[[italic]]", null, Decoration.Italic)]
    [InlineData("[[underline]]", null, Decoration.Underline)]
    [InlineData("[[b,italic]]", null, Decoration.Bold | Decoration.Italic)]
    [InlineData("[[#00FF00]]", "#00FF00", Decoration.None)]
    [InlineData("[[#00FF00,b]]", "#00FF00", Decoration.Bold)]
    public void GetStyle_ShouldParseStylesCorrectly(string input, string? expectedColor, Decoration? expectedDecoration)
    {
        // Act
        var style = MarkupConverter.GetStyle(input);

        // Assert
        if (expectedColor != null)
        {
            var expectedForeground = new Color(0, 255, 0); // #00FF00
            Assert.Equal(expectedForeground, style?.Foreground);
        }
        Assert.Equal(expectedDecoration, style?.Decoration);
    }

    [Fact]
    public void GetStyle_ShouldHandleResetStyle()
    {
        // Arrange
        var input = "[[/]]";

        // Act
        var style = MarkupConverter.GetStyle(input);

        // Assert
        Assert.Null(style);
    }

    [Theory]
    [InlineData("#FF0000", 255, 0, 0)]
    [InlineData("#00FF00", 0, 255, 0)]
    [InlineData("#0000FF", 0, 0, 255)]
    public void GetColor_ShouldReturnCorrectColor(string colorCode, byte expectedR, byte expectedG, byte expectedB)
    {
        // Act
        var color = MarkupConverter.GetColor(colorCode);

        // Assert
        Assert.Equal(expectedR, color.R);
        Assert.Equal(expectedG, color.G);
        Assert.Equal(expectedB, color.B);
    }

    [Theory]
    [InlineData("#ZZZZZZ")]
    [InlineData("#1234")]
    [InlineData("123456")]
    [InlineData("")]
    public void GetColor_ShouldReturnDefaultForInvalidColor(string colorCode)
    {
        // Act & Assert
        Assert.Equal(Color.Default, MarkupConverter.GetColor(colorCode));
    }

    [Fact]
    public void SplitByDoubleBrackets_ShouldSplitCorrectly()
    {
        // Arrange
        var input = "Text before [[style]] styled text [[/]] more text.";

        // Act
        var segments = MarkupConverter.SplitByDoubleBrackets(input);

        // Assert
        Assert.Equal(5, segments.Count);
        Assert.Equal("Text before ", segments[0]);
        Assert.Equal("[[style]]", segments[1]);
        Assert.Equal(" styled text ", segments[2]);
        Assert.Equal("[[/]]", segments[3]);
        Assert.Equal(" more text.", segments[4]);
    }

    [Fact]
    public void SplitByDoubleBrackets_ShouldHandleEmptyInput()
    {
        // Arrange
        var input = "";

        // Act
        var segments = MarkupConverter.SplitByDoubleBrackets(input);

        // Assert
        Assert.Empty(segments);
    }

    [Fact]
    public void SplitByDoubleBrackets_ShouldHandleNoBrackets()
    {
        // Arrange
        var input = "No styles here";

        // Act
        var segments = MarkupConverter.SplitByDoubleBrackets(input);

        // Assert
        Assert.Single(segments);
        Assert.Equal("No styles here", segments[0]);
    }
}
