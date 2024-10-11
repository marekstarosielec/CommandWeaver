using Models.Interfaces.Context;

namespace Models;

public abstract class Operation
{
    public abstract Task Run(IContext context, CancellationToken cancellationToken);
}