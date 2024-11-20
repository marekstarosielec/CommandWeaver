public class OperationConditions(IOutput output) : IOperationConditions
{
    public bool ShouldBeSkipped(OperationCondition? condition, IVariables variables)
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
}

