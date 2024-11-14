using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

internal static class MarkupConverter
{
    public static FormattableString Convert(string input)
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
