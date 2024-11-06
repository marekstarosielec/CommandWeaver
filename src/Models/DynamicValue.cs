using System;
using System.Diagnostics;

namespace Models;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record DynamicValue
{
    public DynamicValue() { }

    public DynamicValue(string? textValue) => TextValue = textValue;

    public DynamicValue(DynamicValueObject? objectValue) => ObjectValue = objectValue;

    public DynamicValue(DynamicValueList? listValue) => ListValue = listValue;

    public string? TextValue { get; set; }

    public DynamicValueObject? ObjectValue { get; set; }

    public DynamicValueList? ListValue { get; set; }

    public bool IsNull => TextValue == null && ObjectValue == null && ListValue == null;
    //public int? NumericValue { get; set; }

    //public bool? BoolValue { get; set; }

    //public decimal? DecimalValue { get; set; }

    //public DateTime? DateTimeValue { get; set; }

    public T? GetEnumValue<T>() where T : struct, Enum =>
        !string.IsNullOrWhiteSpace(TextValue) && Enum.TryParse(TextValue, out T result)
        ? result
        : null;

    private string? DebuggerDisplay =>
    TextValue != null ? $"Text: {TextValue}" :
        ListValue != null ? $"List (Count: {ListValue.ToList().Count})" :
        ObjectValue != null ? $"Object: {ObjectValue[ObjectValue.Keys.First()]?.TextValue}" :
        "No values";
}
