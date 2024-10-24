namespace Models;

public class VariableValue
{
    public string? StringValue { get; set; }

    public int? NumericValue { get; set; } 

    public bool? BoolValue { get; set; }

    public decimal? DecimalValue { get; set; }

    public DateTime? DateTimeValue { get; set; }

    public VariableValueObject? ObjectValue { get; set; }

    public VariableValueList? ListValue { get; set; }

}
