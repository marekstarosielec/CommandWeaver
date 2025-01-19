using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a parameter for a command, typically provided via command line arguments.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public record CommandParameter : IValidatable
{
    public CommandParameter()
    {
    }

    [SetsRequiredMembers]
    public CommandParameter(string key, Type? allowedEnumValues)
    {
        Key = key;
        AllowedEnumValues = allowedEnumValues;
    }

    /// <summary>
    /// Gets or sets the key for the command parameter.
    /// </summary>
    /// <remarks>
    /// This field is required and serves as the unique identifier for the parameter. It is used to 
    /// match the parameter in the command line arguments.
    /// </remarks>
    public required string Key { get; init; }

    /// <summary>
    /// Gets or sets a list of alternative names for the parameter.
    /// </summary>
    /// <remarks>
    /// These names provide flexibility in how the parameter can be specified in command line arguments.
    /// </remarks>
    public virtual ImmutableList<string>? OtherNames { get; init; }

    /// <summary>
    /// Gets or sets a description of the command parameter.
    /// </summary>
    /// <remarks>
    /// This provides additional information or guidance on the parameter's usage, helping users understand 
    /// its purpose and expected input.
    /// </remarks>
    public string? Description { get; init; }

    /// <inheritdoc />
    public bool Required { get; set; }
    
    /// <inheritdoc />
    public ImmutableList<string>? AllowedTextValues { get; init; }
    
    /// <inheritdoc />
    public Type? AllowedEnumValues { get; init; }

    /// <inheritdoc />
    public string? AllowedType { get; set; }

    /// <inheritdoc />
    public bool? List { get; set; }
    
    /// <summary>
    /// Allows to set alternative value (or values) if current value is null.
    /// </summary>
    public DynamicValue IfNull { get; set; } = new ();
    
    public CommandPrompt Prompt { get; init; } = new ();
    
    /*
     * prompt:
     * should it be called when not required and is empty - enabled yes/no, default yes
     * custom description
     * should I use required?
     * AllowedValue or AllowedEnumValues - selection
     * bool value - with predefined default?
     * otherwise input string/int/precision/date
     *
     *
     *
     * prompt operation:
     * custom description
     * required
     * allowed values or allowed enum value
     * allowed type
     */
}
