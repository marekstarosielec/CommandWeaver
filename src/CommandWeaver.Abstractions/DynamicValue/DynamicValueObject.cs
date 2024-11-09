using System.Collections.Immutable;

/// <summary>
/// Represents an immutable dictionary of dynamic values with string keys, allowing dictionary-like access to 
/// dynamic values within the object.
/// </summary>
public record DynamicValueObject
{
    private readonly ImmutableDictionary<string, DynamicValue?> _data = ImmutableDictionary<string, DynamicValue?>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValueObject"/> class with the specified dictionary.
    /// </summary>
    /// <param name="dictionary">A dictionary of string keys and <see cref="DynamicValue"/> values to initialize the object.</param>
    public DynamicValueObject(IDictionary<string, DynamicValue?> dictionary) =>
        _data = dictionary.ToImmutableDictionary();

    /// <summary>
    /// Gets the collection of keys in the dictionary.
    /// </summary>
    public IEnumerable<string> Keys => _data.Keys;

    /// <summary>
    /// Gets the <see cref="DynamicValue"/> associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The <see cref="DynamicValue"/> associated with the specified key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the key does not exist in the dictionary.</exception>
    public DynamicValue this[string key] => _data.TryGetValue(key, out var value)
        ? value ?? throw new ArgumentOutOfRangeException(nameof(key), "Value for the specified key is null.")
        : throw new ArgumentOutOfRangeException(nameof(key), "Specified key does not exist.");
}
