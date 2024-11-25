using System.Collections.Immutable;

/// <inheritdoc />
public class OutputSettings : IOutputSettings
{
    /// <inheritdoc />
    public ImmutableDictionary<string, string>? Styles { get; set; }
    
    /// <inheritdoc />
    public void SetStyles(DynamicValue styles)
    {
        if (styles.ListValue == null)
            return;

        Styles = styles.ListValue
            .Where(style => !string.IsNullOrWhiteSpace(style.ObjectValue?["key"].TextValue) &&
                            !string.IsNullOrWhiteSpace(style.ObjectValue?["value"].TextValue))
            .ToDictionary(style => style.ObjectValue!["key"].TextValue!.ToLower(),
                style => style.ObjectValue!["value"].TextValue!).ToImmutableDictionary();
    }

    /// <inheritdoc />
    public LogLevel CurrentLogLevel { get; set; }
    
    /// <inheritdoc />
    public IJsonSerializer? Serializer { get; set; }
}