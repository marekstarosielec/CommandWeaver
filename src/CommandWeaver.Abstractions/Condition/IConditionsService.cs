/// <summary>
/// Provides a service for evaluating conditions and extracting conditions from dynamic values.
/// </summary>
public interface IConditionsService
{
    /// <summary>
    /// Evaluates whether the specified condition is met based on the provided variables.
    /// </summary>
    /// <param name="condition">The condition to evaluate. Can be <c>null</c>, in which case the method should return <c>true</c>.</param>
    /// <returns>
    /// <c>true</c> if the condition is met or if the condition is <c>null</c>; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is used to determine whether a given operation or item should be executed or included based on the specified condition.
    /// </remarks>
    bool ConditionsAreMet(Condition? condition);

    /// <summary>
    /// Extracts a <see cref="Condition"/> from a <see cref="DynamicValue"/> if applicable.
    /// </summary>
    /// <param name="dynamicValue">The dynamic value that may represent a condition.</param>
    /// <returns>
    /// The extracted <see cref="Condition"/>, or <c>null</c> if the dynamic value does not represent a valid condition.
    /// </returns>
    /// <remarks>
    /// This method converts a dynamic value into a condition, enabling dynamic definitions of conditions at runtime.
    /// </remarks>
    Condition? GetFromDynamicValue(DynamicValue dynamicValue);
}