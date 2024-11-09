using System.Collections;
using System.Collections.Immutable;

/// <summary>
/// Represents an immutable list of <see cref="DynamicValueObject"/> items, with support for functional modifications 
/// and collection-like access.
/// </summary>
public record DynamicValueList : IEnumerable<DynamicValueObject>
{
    private ImmutableList<DynamicValueObject> _items = ImmutableList<DynamicValueObject>.Empty;

    /// <summary>
    /// Initializes an empty instance of the <see cref="DynamicValueList"/> class.
    /// </summary>
    public DynamicValueList() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValueList"/> class with the specified list of items.
    /// </summary>
    /// <param name="items">The list of <see cref="DynamicValueObject"/> items to initialize the list with.</param>
    public DynamicValueList(IList<DynamicValueObject> items) => _items = items.ToImmutableList();

    /// <summary>
    /// Gets the <see cref="DynamicValueObject"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The <see cref="DynamicValueObject"/> at the specified index.</returns>
    public DynamicValueObject this[int index] => _items[index];

    /// <summary>
    /// Returns a new <see cref="DynamicValueList"/> with the specified item added.
    /// </summary>
    /// <param name="item">The <see cref="DynamicValueObject"/> to add.</param>
    /// <returns>A new <see cref="DynamicValueList"/> instance with the item added.</returns>
    public DynamicValueList Add(DynamicValueObject item) => this with { _items = _items.Add(item) };

    /// <summary>
    /// Returns the first element that matches the specified predicate, or the default value if no match is found.
    /// </summary>
    /// <param name="predicate">The predicate to apply to each element.</param>
    /// <returns>The first matching <see cref="DynamicValueObject"/>, or <c>null</c> if no match is found.</returns>
    public DynamicValueObject? FirstOrDefault(Func<DynamicValueObject, bool> predicate) =>
        _items.FirstOrDefault(predicate);

    /// <summary>
    /// Returns the first element in the list, or the default value if the list is empty.
    /// </summary>
    /// <returns>The first <see cref="DynamicValueObject"/>, or <c>null</c> if the list is empty.</returns>
    public DynamicValueObject? FirstOrDefault() => _items.FirstOrDefault();

    /// <summary>
    /// Returns a new <see cref="DynamicValueList"/> with all elements matching the specified predicate removed.
    /// </summary>
    /// <param name="match">The predicate to determine which elements to remove.</param>
    /// <returns>A new <see cref="DynamicValueList"/> instance with matching elements removed.</returns>
    public DynamicValueList RemoveAll(Predicate<DynamicValueObject> match) =>
        this with { _items = _items.RemoveAll(match) };

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="DynamicValueList"/>.
    /// </summary>
    /// <returns>An enumerator for the <see cref="DynamicValueObject"/> collection.</returns>
    public IEnumerator<DynamicValueObject> GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="DynamicValueList"/>.
    /// </summary>
    /// <returns>An enumerator for the <see cref="DynamicValueObject"/> collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
