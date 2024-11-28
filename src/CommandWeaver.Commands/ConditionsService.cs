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
        return true;
    }

    public Condition? GetFromDynamicValue(DynamicValue dynamicValue)
    {
        if (dynamicValue.IsNull() || dynamicValue.ObjectValue == null || !dynamicValue.ObjectValue!.Keys.Any())
            return null;

        DynamicValue? isNull = null;
        DynamicValue? isNotNull = null;
        
        foreach (var property in dynamicValue.ObjectValue!.Keys)
            if (string.Equals(property, "IsNull", StringComparison.OrdinalIgnoreCase))
                isNull = dynamicValue.ObjectValue![property];
            else if (string.Equals(property, "IsNotNull", StringComparison.OrdinalIgnoreCase))
                isNotNull = dynamicValue.ObjectValue![property];
            else
            {
                flow.Terminate($"Unknown condition property: {property}");
                return null;
            }

        return new Condition{ IsNull = isNull, IsNotNull = isNotNull };
    }
}