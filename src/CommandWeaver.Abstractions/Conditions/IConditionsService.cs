public interface IConditionsService
{
    bool ShouldBeSkipped(Condition? condition, IVariables variables);
    
    Condition? GetFromDynamicValue(DynamicValue dynamicValue);
}