public class ConditionsService(IOutputService output, IFlowService flow, IVariableService variables) : IConditionsService
{
    public bool ConditionsAreMet(Condition? condition)
    {
        if (condition?.IsNull != null)
        {
            var result = variables.ReadVariableValue(condition.IsNull);
            if (!result.IsNull())
            {
                //TODO: Conditions are also used for rest headers - need to adjust message
               // output.Trace($"Skipping operation because of IsNull condition.");
                return false;
            }
        }
        if (condition?.IsNotNull != null)
        {
            var result = variables.ReadVariableValue(condition.IsNotNull);
            if (result.IsNull())
            {
             //   output.Trace($"Skipping operation because of IsNotNull condition.");
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
            //TODO: unify StringComparison
            if (string.Equals(property, "IsNull", StringComparison.OrdinalIgnoreCase))
                isNull = dynamicValue.ObjectValue![property];
            else if (string.Equals(property, "IsNotNull", StringComparison.OrdinalIgnoreCase))
                isNotNull = dynamicValue.ObjectValue![property];
            else 
                //TODO: provide as much information as possible
                flow.Terminate($"Unknown condition {property}");
        return new Condition{ IsNull = isNull, IsNotNull = isNotNull };
    }
}

