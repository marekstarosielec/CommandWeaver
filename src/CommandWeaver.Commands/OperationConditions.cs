public interface IOperationConditions
{
    bool OperationShouldBeSkipped(Operation operation, IVariables variables);
}

public class OperationConditions(IOutput output) : IOperationConditions
{
    public bool OperationShouldBeSkipped(Operation operation, IVariables variables)
    {
        if (operation.Conditions?.IsNull != null)
        {
            var result = variables.ReadVariableValue(operation.Conditions.IsNull);
            if (!result.IsNull())
            {
                output.Trace($"Skipping operation {operation.Name} because of IsNull condition.");
                return true;
            }
        }
        if (operation.Conditions?.IsNotNull != null)
        {
            var result = variables.ReadVariableValue(operation.Conditions.IsNotNull);
            if (result.IsNull())
            {
                output.Trace($"Skipping operation {operation.Name} because of IsNotNull condition.");
                return true;
            }
        }
        return false;
    }
}

