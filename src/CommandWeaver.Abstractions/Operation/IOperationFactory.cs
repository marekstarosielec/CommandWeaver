/// <summary>
/// Defines a factory for creating instances of <see cref="Operation"/> based on a specified name.
/// </summary>
public interface IOperationFactory
{
    /// <summary>
    /// Retrieves an operation instance by its name.
    /// </summary>
    /// <param name="name">The name of the operation to retrieve.</param>
    /// <returns>
    /// An instance of <see cref="Operation"/> if an operation with the specified name exists;
    /// otherwise, <c>null</c> if no matching operation is found.
    /// </returns>
    Operation? GetOperation(string? name);
}
