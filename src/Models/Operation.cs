using Models.Interfaces.Context;

namespace Models;

public abstract class Operation
{
    public abstract string Name { get; }

    public abstract Dictionary<string, OperationParameter> Parameters { get; }

    public abstract Task Run(IContext context, CancellationToken cancellationToken);
}