using System.Collections.Immutable;

namespace Models;

public record DynamicValueObject
{
    private ImmutableDictionary<string, DynamicValue?> _data = ImmutableDictionary<string, DynamicValue?>.Empty;

    // Constructor that creates empty object
    public DynamicValueObject() { }

    // Constructor that accepts a regular Dictionary
    public DynamicValueObject(IDictionary<string, DynamicValue?> dictionary) => _data = dictionary.ToImmutableDictionary();


    // Indexer for dictionary-like access
    public DynamicValue this[string key] => _data[key] ?? throw new ArgumentOutOfRangeException(nameof(key));

    public IEnumerable<string> Keys { get => _data.Keys; }

    /// <summary>
    /// Creates a copy of object with modified element.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public DynamicValueObject With(string key, DynamicValue? value) => new DynamicValueObject(_data.SetItem(key, value));

}