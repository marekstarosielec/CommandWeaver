using System.Diagnostics;

namespace Models;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record VariableValue
{
    public VariableValue() { }

    public VariableValue(string? textValue) => TextValue = textValue;

    public VariableValue(VariableValueObject? objectValue) => ObjectValue = objectValue;

    public VariableValue(VariableValueList? listValue) => ListValue = listValue;

    public string? TextValue { get; set; }

    public VariableValueObject? ObjectValue { get; set; }

    public VariableValueList? ListValue { get; set; }

    //public int? NumericValue { get; set; }

    //public bool? BoolValue { get; set; }

    //public decimal? DecimalValue { get; set; }

    //public DateTime? DateTimeValue { get; set; }

    private string DebuggerDisplay =>
        TextValue != null ? $"Text: {TextValue}" :
        ListValue != null ? $"List (Count: {ListValue.ToList().Count})" :
        ObjectValue != null ? $"Object: {ObjectValue["key"]?.TextValue}" :
        "Null";
}
