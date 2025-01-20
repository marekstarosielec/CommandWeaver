using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

/// <summary>
/// Represents a JSON array as an immutable list of <see cref="DynamicValue"/> items. 
/// This class provides functional modification methods, collection-like access, and ensures immutability 
/// for handling dynamic and nested JSON array structures.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record DynamicValueList : IEnumerable<DynamicValue>
{
    private ImmutableList<DynamicValue> _items = ImmutableList<DynamicValue>.Empty;

    /// <summary>
    /// Initializes an empty instance of the <see cref="DynamicValueList"/> class.
    /// </summary>
    public DynamicValueList() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValueList"/> class with the specified list of items.
    /// </summary>
    /// <param name="items">The list of <see cref="DynamicValue"/> items to initialize the list with.</param>
    public DynamicValueList(IList<DynamicValue> items) =>
        _items = items as ImmutableList<DynamicValue> ?? items.ToImmutableList();
            

    /// <summary>
    /// Gets the <see cref="DynamicValue"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The <see cref="DynamicValue"/> at the specified index.</returns>
    public DynamicValue this[int index] => _items[index];

    /// <summary>
    /// Returns a new <see cref="DynamicValueList"/> with the specified item added.
    /// </summary>
    /// <param name="item">The <see cref="DynamicValue"/> to add.</param>
    /// <returns>A new <see cref="DynamicValueList"/> instance with the item added.</returns>
    public DynamicValueList Add(DynamicValue item) => this with { _items = _items.Add(item) };

    /// <summary>
    /// Returns a new <see cref="DynamicValueList"/> with the specified items added.
    /// </summary>
    /// <param name="items">The items to add to the list.</param>
    /// <returns>A new <see cref="DynamicValueList"/> instance with the items added.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided items are null.</exception>
    public DynamicValueList AddRange(IEnumerable<DynamicValue> items) =>
        this with { _items = _items.AddRange(items ?? throw new ArgumentNullException(nameof(items), "Items cannot be null.")) };

    /// <summary>
    /// Returns the first element that matches the specified predicate, or <c>null</c> if no match is found.
    /// </summary>
    /// <param name="predicate">The predicate to apply to each element.</param>
    /// <returns>The first matching <see cref="DynamicValue"/>, or <c>null</c> if no match is found.</returns>
    public DynamicValue? FirstOrDefault(Func<DynamicValue, bool> predicate) =>
        _items.FirstOrDefault(predicate);

    /// <summary>
    /// Returns the first element in the list, or <c>null</c> if the list is empty.
    /// </summary>
    /// <returns>The first <see cref="DynamicValue"/>, or <c>null</c> if the list is empty.</returns>
    public DynamicValue? FirstOrDefault() => _items.FirstOrDefault();

    /// <summary>
    /// Returns a new <see cref="DynamicValueList"/> with all elements matching the specified predicate removed.
    /// </summary>
    /// <param name="match">The predicate to determine which elements to remove.</param>
    /// <returns>A new <see cref="DynamicValueList"/> instance with matching elements removed.</returns>
    public DynamicValueList RemoveAll(Predicate<DynamicValue> match) =>
        this with { _items = _items.RemoveAll(match) };

    /// <summary>
    /// Gets the number of items in the list.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="DynamicValueList"/>.
    /// </summary>
    /// <returns>An enumerator for the <see cref="DynamicValue"/> collection.</returns>
    public IEnumerator<DynamicValue> GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="DynamicValueList"/>.
    /// </summary>
    /// <returns>An enumerator for the <see cref="DynamicValue"/> collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Provides a string representation of the list for debugging purposes.
    /// </summary>
    private string DebuggerDisplay =>
        $"List with {Count} items: [{string.Join(", ", _items.Take(5))}{(Count > 5 ? ", ..." : "")}]";
}
