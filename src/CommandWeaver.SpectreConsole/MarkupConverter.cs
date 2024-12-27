using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;
using Spectre.Console.Rendering;

internal static class MarkupConverter
{
    private static readonly Regex SplitRegex = new(@"(\[\[.*?\]\])", RegexOptions.Compiled);

    private static readonly Dictionary<string, Decoration> Decorations = new()
    {
        { "b", Decoration.Bold },
        { "bold", Decoration.Bold },
        { "i", Decoration.Italic },
        { "italic", Decoration.Italic },
        { "u", Decoration.Underline },
        { "underline", Decoration.Underline },
    };

    //TODO: Add unit tests.
    public static string ConvertToSpectreMarkup(string input)
    {
        var markup = new StringBuilder(input.Length);
        foreach (var segment in ConvertToSegments(input))
        {
            var style = segment.Style.ToMarkup();
            if (string.IsNullOrWhiteSpace(style))
                style = "/";
            markup.Append($"[{style}]{segment.Text}");
        }

        markup.Append("[/]");
        var result = markup.ToString();
        if (result.StartsWith("[/]"))
            result = result.Substring(3);
        return result;
    }

    //TODO: Add unit tests for sad paths (e.g. multiple style tags), not it throws InvalidOperationException.
    //TODO: Add unit tests.
    public static string ConvertToSpectreStyle(string input) => GetStyle(input).ToMarkup();

    public static IEnumerable<Segment> ConvertToSegments(string input)
    {
        Style? style = null;
        var segments = SplitByDoubleBrackets(input);
        foreach (var segment in segments)
            if (segment.StartsWith("[[") && segment.EndsWith("]]"))
                style = GetStyle(segment);
            else
            {
                //If styling was replaced, here it is replaced back.
                var value = segment.Replace("/[/[", "[[").Replace("/]/]", "]]");
                yield return style == null ? new Segment(value) : new Segment(value, style);
            }
    }

    internal static Style GetStyle(string input)
    {
        var style = input.TrimStart('[').TrimEnd(']');
        if (style == "/")
            return new Style();

        Color? color = null;
        Decoration? decoration = null;
        var styles = style.Split(',');

        foreach (var styleSection in styles)
        {
            if (styleSection.StartsWith("#"))
                color = GetColor(styleSection);
            else if (Decorations.TryGetValue(styleSection.ToLowerInvariant(), out var decor))
                decoration = decoration.HasValue ? decoration | decor : decor;
        }

        return new Style(foreground: color, decoration: decoration);
    }

    internal static Color GetColor(string rgb)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rgb) || rgb.Length != 7 || rgb[0] != '#')
                throw new ArgumentException("Invalid RGB text format. Must be in #RRGGBB format", nameof(rgb));

            return new Color(
                Convert.ToByte(rgb.Substring(1, 2), 16),
                Convert.ToByte(rgb.Substring(3, 2), 16),
                Convert.ToByte(rgb.Substring(5, 2), 16));
        }
        catch
        {
            return Color.Default; // Fallback to default color
        }
    }

    internal static List<string> SplitByDoubleBrackets(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new List<string>();

        var segments = new List<string>(SplitRegex.Split(input));
        segments.RemoveAll(string.IsNullOrEmpty); // Clean up empty segments
        return segments;
    }
}
