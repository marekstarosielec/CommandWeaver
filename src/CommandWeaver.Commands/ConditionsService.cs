public class ConditionsService(IOutputService output, IFlowService flow, IVariableService variables) : IConditionsService
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
            if (!areEqualResult)
            {
                output.Trace($"Condition 'AreEqual' not met for variable: {condition.AreEqual}");
                return false;
            }
            return areEqualResult;
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
            else
            {
                flow.Terminate($"Unknown condition property: {property}");
                return null;
            }

        return new Condition{ IsNull = isNull, IsNotNull = isNotNull, AreEqual = areEqual };
    }
}