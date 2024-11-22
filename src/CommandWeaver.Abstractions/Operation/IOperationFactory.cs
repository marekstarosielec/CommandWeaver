/// <summary>
/// Defines a factory for creating and managing instances of <see cref="Operation"/> based on their names.
/// </summary>
public interface IOperationFactory
{
    /// <summary>
    /// Retrieves an operation instance by its name.
    /// </summary>
    /// <param name="name">The name of the operation to retrieve. Can be <c>null</c>.</param>
    /// <returns>
    /// An instance of <see cref="Operation"/> if an operation with the specified name exists;
    /// otherwise, <c>null</c> if no matching operation is found.
    /// </returns>
    /// <remarks>
    /// This method allows operations to be dynamically retrieved by name, enabling flexible and extensible operation management.
    /// </remarks>
    Operation? GetOperation(string? name);

    /// <summary>
    /// Gets a dictionary of available operations managed by the factory.
    /// </summary>
    /// <value>
    /// A dictionary where the keys are operation names and the values are instances of <see cref="Operation"/>.
    /// </value>
    /// <remarks>
    /// This property provides access to all operations currently managed by the factory, allowing inspection or iteration over the available operations.
    /// </remarks>
    Dictionary<string, Operation> Operations { get; }
}