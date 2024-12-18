using System.Diagnostics;
using System.Globalization;

/// <summary>
/// Represents a dynamic value capable of storing multiple types, including text, date-time, boolean, numeric, 
/// precision, object, or list values.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record DynamicValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with no initial value.
    /// </summary>
    public DynamicValue() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a text value.
    /// </summary>
    /// <param name="textValue">The string value to be assigned.</param>
    /// <param name="noResolving">Indicates whether resolving stops for this value.</param>
    public DynamicValue(string? textValue, bool noResolving = false)
    {
        TextValue = textValue;
        NoResolving = noResolving;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a date-time value.
    /// </summary>
    /// <param name="dateTimeValue">The <see cref="DateTimeOffset"/> value to be assigned.</param>
    public DynamicValue(DateTimeOffset dateTimeValue) => DateTimeValue = dateTimeValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a boolean value.
    /// </summary>
    /// <param name="boolValue">The boolean value to be assigned.</param>
    public DynamicValue(bool boolValue) => BoolValue = boolValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a numeric value.
    /// </summary>
    /// <param name="numericValue">The long integer value to be assigned.</param>
    public DynamicValue(long numericValue) => NumericValue = numericValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a precision value.
    /// </summary>
    /// <param name="precisionValue">The double-precision value to be assigned.</param>
    public DynamicValue(double precisionValue) => PrecisionValue = precisionValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with an object value.
    /// </summary>
    /// <param name="objectValue">The dictionary of string keys and <see cref="DynamicValue"/> values to be assigned.</param>
    public DynamicValue(IDictionary<string, DynamicValue?> objectValue) => ObjectValue = new DynamicValueObject(objectValue);

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a <see cref="DynamicValueObject"/> value.
    /// </summary>
    /// <param name="objectValue">The <see cref="DynamicValueObject"/> instance to be assigned.</param>
    public DynamicValue(DynamicValueObject? objectValue) => ObjectValue = objectValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a list of <see cref="DynamicValue"/> values.
    /// </summary>
    /// <param name="listValue">The list of <see cref="DynamicValue"/> items to be assigned.</param>
    public DynamicValue(List<DynamicValue> listValue) => ListValue = new DynamicValueList(listValue);

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a <see cref="DynamicValueList"/> value.
    /// </summary>
    /// <param name="listValue">The <see cref="DynamicValueList"/> instance to be assigned.</param>
    public DynamicValue(DynamicValueList listValue) => ListValue = listValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a lazy loaded text value.
    /// </summary>
    /// <param name="lazyTextValue">The lazy loaded text value to be assigned.</param>
    public DynamicValue(Lazy<string?> lazyTextValue) => LazyTextValue = lazyTextValue;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a lazy loaded binary value.
    /// </summary>
    /// <param name="lazyBinaryValue">The lazy loaded binary value to be assigned.</param>
    public DynamicValue(Lazy<byte[]?> lazyBinaryValue) => LazyBinaryValue = lazyBinaryValue;
    
    /// <summary>
    /// Gets the text value of the dynamic value.
    /// </summary>
    public string? TextValue { get; }

    /// <summary>
    /// Gets the date-time value of the dynamic value.
    /// </summary>
    public DateTimeOffset? DateTimeValue { get; }

    /// <summary>
    /// Gets the boolean value of the dynamic value.
    /// </summary>
    public bool? BoolValue { get; }

    /// <summary>
    /// Gets the numeric value of the dynamic value.
    /// </summary>
    public long? NumericValue { get; }

    /// <summary>
    /// Gets the precision value of the dynamic value.
    /// </summary>
    public double? PrecisionValue { get; }

    /// <summary>
    /// Gets the object value of the dynamic value.
    /// </summary>
    public DynamicValueObject? ObjectValue { get; }

    /// <summary>
    /// Gets the list value of the dynamic value.
    /// </summary>
    public DynamicValueList? ListValue { get; }

    /// <summary>
    /// Gets the text value that is loaded on request. Used for file content.
    /// </summary>
    public Lazy<string?>? LazyTextValue { get; }
    
    /// <summary>
    /// Gets the binary value that is loaded on request. Used for file content.
    /// </summary>
    public Lazy<byte[]?>? LazyBinaryValue { get; }

    /// <summary>
    /// Indicates that the value may contain unresolved metatags (e.g., {{ name }}) but they should not be replaced 
    /// or resolved. This is useful for cases where the raw value needs to be preserved, such as when exporting or 
    /// displaying the original data.
    /// </summary>
    public bool NoResolving { get; init; }
    
    /// <summary>
    /// Determines whether the dynamic value is null, meaning all properties are unassigned.
    /// </summary>
    /// <returns><c>true</c> if the dynamic value is null; otherwise, <c>false</c>.</returns>
    public bool IsNull() =>
        TextValue is null &&
        DateTimeValue is null &&
        BoolValue is null &&
        NumericValue is null &&
        PrecisionValue is null &&
        ObjectValue is null &&
        ListValue is null;

    /// <summary>
    /// Attempts to retrieve an enum value from the stored text value if it matches the specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type to attempt parsing.</typeparam>
    /// <returns>The parsed enum value if successful; otherwise, <c>null</c>.</returns>
    public T? GetEnumValue<T>() where T : struct, Enum =>
        !string.IsNullOrWhiteSpace(TextValue) && Enum.TryParse(TextValue, true, out T result)
        ? result
        : null;

    /// <summary>
    /// Returns first encountered value as text. 
    /// </summary>
    /// <returns></returns>
    public string? GetTextValue()
    {
        if (!string.IsNullOrWhiteSpace(TextValue))
            return TextValue;
        if (BoolValue.HasValue)
            return BoolValue.Value ? "true" : "false";
        if (NumericValue.HasValue)
            return NumericValue.Value.ToString(CultureInfo.InvariantCulture);
        if (PrecisionValue.HasValue)
            return PrecisionValue.Value.ToString(CultureInfo.InvariantCulture);
        if (DateTimeValue.HasValue)
            return DateTimeValue.Value.ToString(CultureInfo.InvariantCulture);
        if (ListValue != null)
            foreach (var value in ListValue)
            {
                var listTextValue = value.GetTextValue();
                if (!string.IsNullOrWhiteSpace(listTextValue))
                    return listTextValue;
            }
        return null;
    }
    
    /// <summary>
    /// Provides a debug string representation of the dynamic value for debugging purposes.
    /// </summary>
    internal string DebuggerDisplay
    {
        get
        {
            if (TextValue != null) return $"Text: \"{TextValue}\"";
            if (DateTimeValue != null) return $"DateTime: {DateTimeValue}";
            if (BoolValue != null) return $"Bool: {BoolValue}";
            if (NumericValue != null) return $"Numeric: {NumericValue}";
            if (PrecisionValue != null) return $"Precision: {PrecisionValue.Value.ToString(CultureInfo.InvariantCulture)}";
            if (ObjectValue != null)
            {
                var preview = ObjectValue.Keys.Contains("key") && ObjectValue["key"].TextValue != null
                    ? ObjectValue["key"].TextValue
                    : "no key";
                return $"Object: {preview}";
            }
            return ListValue != null ? $"List (Count: {ListValue.ToList().Count})" : "No values";
        }
    }
}
