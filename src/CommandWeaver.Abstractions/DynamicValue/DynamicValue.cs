using System.Diagnostics;

/// <summary>
/// Represents a dynamic value that can store various types, including text, date-time, boolean, numeric, precision,
/// object, or list values.
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
    public DynamicValue(string? textValue) => TextValue = textValue;

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
    /// <param name="precisionValue">The double precision value to be assigned.</param>
    public DynamicValue(double precisionValue) => PrecisionValue = precisionValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with an object value.
    /// </summary>
    /// <param name="objectValue">The dictionary of string keys and <see cref="DynamicValue"/> values to be assigned.</param>
    public DynamicValue(IDictionary<string, DynamicValue?> objectValue) => ObjectValue = new DynamicValueObject(objectValue);

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a <see cref="DynamicValueObject"/> value.
    /// </summary>
    /// <param name="objectValue">The <see cref="DynamicValueObject"/> to be assigned.</param>
    public DynamicValue(DynamicValueObject? objectValue) => ObjectValue = objectValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a list of <see cref="DynamicValueObject"/> values.
    /// </summary>
    /// <param name="listValue">The list of <see cref="DynamicValueObject"/> values to be assigned.</param>
    public DynamicValue(List<DynamicValueObject> listValue) => ListValue = new DynamicValueList(listValue);

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicValue"/> class with a <see cref="DynamicValueList"/> value.
    /// </summary>
    /// <param name="listValue">The <see cref="DynamicValueList"/> value to be assigned.</param>
    public DynamicValue(DynamicValueList listValue) => ListValue = listValue;


    /// <summary>
    /// Gets or sets the text value of the dynamic value.
    /// </summary>
    public string? TextValue { get; }

    /// <summary>
    /// Gets or sets the date-time value of the dynamic value.
    /// </summary>
    public DateTimeOffset? DateTimeValue { get; }

    /// <summary>
    /// Gets or sets the boolean value of the dynamic value.
    /// </summary>
    public bool? BoolValue { get; }

    /// <summary>
    /// Gets or sets the numeric value of the dynamic value.
    /// </summary>
    public long? NumericValue { get; }

    /// <summary>
    /// Gets or sets the precision value of the dynamic value.
    /// </summary>
    public double? PrecisionValue { get; }

    /// <summary>
    /// Gets or sets the object value of the dynamic value.
    /// </summary>
    public DynamicValueObject? ObjectValue { get; }

    /// <summary>
    /// Gets or sets the list value of the dynamic value.
    /// </summary>
    public DynamicValueList? ListValue { get; }

    /// <summary>
    /// Gets a value indicating whether the dynamic value is null (i.e., all properties are unassigned).
    /// </summary>
    public bool IsNull() => TextValue == null && DateTimeValue == null && BoolValue == null && NumericValue == null && PrecisionValue == null && ObjectValue == null && ListValue == null;

    /// <summary>
    /// Attempts to retrieve an enum value from the stored text value if it matches the specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type to attempt parsing.</typeparam>
    /// <returns>The parsed enum value if successful; otherwise, null.</returns>
    public T? GetEnumValue<T>() where T : struct, Enum =>
        !string.IsNullOrWhiteSpace(TextValue) && Enum.TryParse(TextValue, out T result)
        ? result
        : null;

    /// <summary>
    /// Provides a debug string representation of the dynamic value for debugging purposes.
    /// </summary>
    internal string? DebuggerDisplay
    {
        get
        {
            if (TextValue != null) return $"Text: \"{TextValue}\"";
            if (DateTimeValue != null) return $"DateTime: {DateTimeValue}";
            if (BoolValue != null) return $"Bool: {BoolValue}";
            if (NumericValue != null) return $"Numeric: {NumericValue}";
            if (PrecisionValue != null) return $"Precision: {PrecisionValue}";
            if (ObjectValue != null) return $"Object: {ObjectValue}";
            if (ListValue != null) return $"List (Count: {ListValue.ToList().Count})";

            return "No values";
        }
    }
}
