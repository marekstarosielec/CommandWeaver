using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a parameter for a command, typically provided via command line arguments.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public record CommandParameter
{
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

    /// <summary>
    /// Gets or sets the list of allowed values for the command parameter.
    /// </summary>
    /// <remarks>
    /// If specified, only these values are accepted for the parameter. Values outside this list will result 
    /// in validation errors during parsing.
    /// </remarks>
    public ImmutableList<string>? AllowedValues { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this command parameter is required.
    /// </summary>
    /// <remarks>
    /// If <c>true</c>, the parameter must be provided in the command line arguments; otherwise, it is optional.
    /// </remarks>
    public bool Required { get; set; }
    
    /// <summary>
    /// Gets or sets the type of the allowed enum values for the parameter.
    /// </summary>
    /// <remarks>
    /// If specified, the parameter value must match one of the values in this enum type. This allows enforcing 
    /// strict validation for predefined sets of values.
    /// </remarks>
    public Type? AllowedEnumValues { get; init; }
}
