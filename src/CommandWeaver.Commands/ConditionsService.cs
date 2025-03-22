public class ConditionsService(IOutputService output, IVariableService variables) : IConditionsService
{
    public bool ConditionsAreMet(Condition? condition)
    {
        if (condition?.IsNull != null)
        {
            var result = variables.ReadVariableValue(condition.IsNull);
            if (!result.IsNull())
            {
                output.Trace($"Condition 'IsNull' not met for variable: {condition.IsNull}");
                return false;
            }
        }
        if (condition?.IsNotNull != null)
        {
            var result = variables.ReadVariableValue(condition.IsNotNull);
            if (result.IsNull())
            {
                output.Trace($"Condition 'IsNotNull' not met for variable: {condition.IsNotNull}");
                return false;
            }
        }
        if (condition?.AreEqual != null)
        {
            var value1 = variables.ReadVariableValue(condition.AreEqual.Value1);
            var value2 = variables.ReadVariableValue(condition.AreEqual.Value2);
            var areEqualResult = value1 == value2;
            if (areEqualResult) return areEqualResult;
            output.Trace($"Condition 'AreEqual' not met for variable: {condition.AreEqual}");
            return false;
        }
        if (condition?.AreNotEqual != null)
        {
            var value1 = variables.ReadVariableValue(condition.AreNotEqual.Value1);
            var value2 = variables.ReadVariableValue(condition.AreNotEqual.Value2);
            var areNotEqualResult = value1 != value2;
            if (areNotEqualResult) return areNotEqualResult;
            output.Trace($"Condition 'AreNotEqual' not met for variable: {condition.AreNotEqual}");
            return false;
        }
        return true;
    }

    public Condition? GetFromDynamicValue(DynamicValue dynamicValue)
    {
        if (dynamicValue.IsNull() || dynamicValue.ObjectValue == null || !dynamicValue.ObjectValue!.Keys.Any())
            return null;

        DynamicValue? isNull = null;
        DynamicValue? isNotNull = null;
        DoubleValue? areEqual = null;
        DoubleValue? areNotEqual = null;

        foreach (var property in dynamicValue.ObjectValue!.Keys)
            if (string.Equals(property, "IsNull", StringComparison.OrdinalIgnoreCase))
                isNull = dynamicValue.ObjectValue![property];
            else if (string.Equals(property, "IsNotNull", StringComparison.OrdinalIgnoreCase))
                isNotNull = dynamicValue.ObjectValue![property];
            else if (string.Equals(property, "areEqual", StringComparison.OrdinalIgnoreCase))
                areEqual = new DoubleValue
                {
                    Value1 = dynamicValue.ObjectValue![property].ObjectValue?["value1"] ?? new DynamicValue(),
                    Value2 = dynamicValue.ObjectValue![property].ObjectValue?["value2"] ?? new DynamicValue()
                };
            else if (string.Equals(property, "areNotEqual", StringComparison.OrdinalIgnoreCase))
                areNotEqual = new DoubleValue
                {
                    Value1 = dynamicValue.ObjectValue![property].ObjectValue?["value1"] ?? new DynamicValue(),
                    Value2 = dynamicValue.ObjectValue![property].ObjectValue?["value2"] ?? new DynamicValue()
                };
            else
                throw new CommandWeaverException($"Unknown condition property: {property}");

        return new Condition{ IsNull = isNull, IsNotNull = isNotNull, AreEqual = areEqual, AreNotEqual = areNotEqual};
    }
}