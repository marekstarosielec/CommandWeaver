public class ConditionsService(IOutput output, IFlow flow) : IConditionsService
{
    public bool ShouldBeSkipped(Condition? condition, IVariables variables)
    {
        if (condition?.IsNull != null)
        {
            var result = variables.ReadVariableValue(condition.IsNull);
            if (!result.IsNull())
            {
                //TODO: Conditions are also used for rest headers - need to adjust message
               // output.Trace($"Skipping operation because of IsNull condition.");
                return true;
            }
        }
        if (condition?.IsNotNull != null)
        {
            var result = variables.ReadVariableValue(condition.IsNotNull);
            if (result.IsNull())
            {
             //   output.Trace($"Skipping operation because of IsNotNull condition.");
                return true;
            }
        }
        return false;
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

