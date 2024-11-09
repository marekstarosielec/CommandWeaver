using Models;
using System.Text.Json;

/// <summary>
/// Converts a JSON value to a <see cref="DynamicValue"/> instance and vice versa.
/// </summary>
public interface IDynamicValueConverter : IConverter<DynamicValue?>
{ 
    /// <summary>
    /// Reads a <see cref="JsonElement"/> and converts it to a <see cref="DynamicValue"/>.
    /// </summary>
    /// <param name="element">The JSON element to read.</param>
    /// <returns>A <see cref="DynamicValue"/> instance representing the element data.</returns>
    DynamicValue? ReadElement(JsonElement element);
}