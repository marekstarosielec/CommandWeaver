public interface IOperationConditions
{
    bool ShouldBeSkipped(OperationCondition? condition, IVariables variables);
}