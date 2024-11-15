using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Spectre.Console;
using Spectre.Console.Rendering;

internal static class MarkupConverter
{
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

        yield return new Segment(Environment.NewLine);
    }

    private static Style? GetStyle(string input)
    {
        var style = input.TrimStart('[').TrimEnd(']');
        if (style == "/")
            return null;

        Color? color = null;
        Decoration? decoration = null;
        var styles = style.Split(',');
        foreach (var styleSection in styles)
        {
            if (styleSection.StartsWith("#"))
                color = GetColor(styleSection);
            else if (styleSection.Equals("underline", StringComparison.OrdinalIgnoreCase) ||
                styleSection.Equals("u", StringComparison.OrdinalIgnoreCase))
            {
                decoration ??= new Decoration();
                decoration |= Decoration.Underline;
            }
            else if (styleSection.Equals("bold", StringComparison.OrdinalIgnoreCase) ||
                styleSection.Equals("b", StringComparison.OrdinalIgnoreCase))
            {
                decoration ??= new Decoration();
                decoration |= Decoration.Bold;
            }
            else if (styleSection.Equals("italic", StringComparison.OrdinalIgnoreCase) ||
                      styleSection.Equals("i", StringComparison.OrdinalIgnoreCase))
            {
                decoration ??= new Decoration();
                decoration |= Decoration.Italic;
            }
        }
        return new Style(foreground: color, decoration: decoration);
        
    }

    private static Color GetColor(string rgb)
    {
        if (string.IsNullOrWhiteSpace(rgb) || rgb.Length != 7 || rgb[0] != '#')
            throw new ArgumentException("Invalid RGB text format. Must be in #RRGGBB format", nameof(rgb));

        return new Color(
            Convert.ToByte(rgb.Substring(1, 2), 16),
            Convert.ToByte(rgb.Substring(2, 2), 16),
            Convert.ToByte(rgb.Substring(5, 2), 16));
    }
    public static List<string> SplitByDoubleBrackets(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new List<string>();

        // Regex to split by [[...]] while keeping [[...]] as separate segments
        var pattern = @"(\[\[.*?\]\])";
        var segments = new List<string>(Regex.Split(input, pattern));

        // Remove empty segments, if any
        segments.RemoveAll(string.IsNullOrEmpty);

        return segments;
    }
    
    public static FormattableString Convert2(string input)
    {
        // Initialize the format string and arguments list
        var formatSegments = new List<string>();
        var arguments = new List<object>();
        int lastIndex = 0;

        // Use Regex to find all instances of [[...]]
        var matches = Regex.Matches(input, @"\[\[(.*?)\]\]");
        foreach (Match match in matches)
        {
            // Add preceding static text as a segment
            if (match.Index > lastIndex)
            {
                formatSegments.Add("{" + arguments.Count + "}");  // Use a numbered placeholder
                arguments.Add(input.Substring(lastIndex, match.Index - lastIndex));
            }

            // Process and add the matched tag content as a dynamic argument
            var processedTag = ProcessTag(match.Groups[1].Value);
            formatSegments.Add(processedTag);
            lastIndex = match.Index + match.Length;
        }

        // Add the remaining static text after the last match
        if (lastIndex < input.Length)
        {
            formatSegments.Add("{" + arguments.Count + "}");
            arguments.Add(input.Substring(lastIndex));
        }

        // Create the final FormattableString
        var result = FormattableStringFactory.Create(string.Join("", formatSegments), arguments.ToArray());
        return result;
    }

    // Define a function to process each matched element
    static string ProcessTag(string tagContent)
    {
        var parts = tagContent.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = parts[i].Trim();

            // Custom rules for translating elements
            if (parts[i].StartsWith("#"))
            {
                //Color
            }
            else if (parts[i].Equals("underline", StringComparison.OrdinalIgnoreCase) || parts[i].Equals("u", StringComparison.OrdinalIgnoreCase))
                parts[i] = "underline";
            else if (parts[i].Equals("bold", StringComparison.OrdinalIgnoreCase) || parts[i].Equals("b", StringComparison.OrdinalIgnoreCase))
                parts[i] = "bold";
            else if (parts[i].Equals("italic", StringComparison.OrdinalIgnoreCase) || parts[i].Equals("i", StringComparison.OrdinalIgnoreCase))
                parts[i] = "italic";
            else if (parts[i].Equals("/", StringComparison.OrdinalIgnoreCase))
            { }
            else if (parts[i].Equals("raw", StringComparison.OrdinalIgnoreCase))
            { }
            else
                throw new InvalidOperationException($"Unknown text formatter: {parts[i]}");
        }

        return $"[{string.Join(" ", parts)}]";
    }
}
