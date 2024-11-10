public class TestOperation(string name = "testOperation") : Operation
{
    public override string Name { get; } = name;

    // Make Parameters writable by using a backing field.
    private readonly Dictionary<string, OperationParameter> _parameters = new();
    public override Dictionary<string, OperationParameter> Parameters => _parameters;

    // Conditions can’t be overridden, so we use the base property directly.
    // This will allow setting individual properties on the existing Conditions instance.

    // Provide a no-op Run method, as we don’t need actual functionality in tests.
    public override Task Run(CancellationToken cancellationToken) => Task.CompletedTask;
}
