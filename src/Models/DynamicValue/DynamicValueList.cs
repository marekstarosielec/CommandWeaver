using System.Collections;
using System.Collections.Immutable;

namespace Models;

public record DynamicValueList : IEnumerable<DynamicValueObject>
{
    private ImmutableList<DynamicValueObject> _items = ImmutableList<DynamicValueObject>.Empty;


    public DynamicValueList() { }

    public DynamicValueList(IList<DynamicValueObject> items) => _items = items.ToImmutableList();

    
    // Indexer to access list elements
    public DynamicValueObject this[int index] => _items[index];

    
    // Method to add items to the list (returns a new record with updated list)
    public DynamicValueList Add(DynamicValueObject item) => this with { _items = _items.Add(item) };

    // Method to find the first element or default value
    public DynamicValueObject? FirstOrDefault(Func<DynamicValueObject, bool> predicate) => _items.FirstOrDefault(predicate);

    // Method to find the first element or default value
    public DynamicValueObject? FirstOrDefault() => _items.FirstOrDefault();

    // Method to remove all matching elements
    public DynamicValueList RemoveAll(Predicate<DynamicValueObject> match) => this with { _items = _items.RemoveAll(match) };


    // Implementing GetEnumerator to support foreach
    public IEnumerator<DynamicValueObject> GetEnumerator() => _items.GetEnumerator();

    // Explicit implementation for IEnumerable
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
