using System.Collections.Immutable;

/// <summary>
/// Represents an aggregate operation that can contain and manage a collection of sub-operations.
/// </summary>
/// <remarks>
/// This is a variation of the <see cref="Operation"/> record that allows grouping multiple sub-operations.
/// For example, an aggregate operation such as a loop can execute its sub-operations multiple times.
/// </remarks>
public abstract record OperationAggregate : Operation
{
    /// <summary>
    /// Gets or sets the list of sub-operations contained within this aggregate operation.
    /// </summary>
    /// <remarks>
    /// Each sub-operation represents a distinct task or action that is executed as part of the aggregate operation.
    /// The aggregate operation may define how and when these sub-operations are executed, such as iteratively in a loop.
    /// </remarks>
    public ImmutableList<Operation> Operations { get; set; } = [];
}