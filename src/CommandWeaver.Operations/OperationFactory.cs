using Microsoft.Extensions.DependencyInjection;

public class OperationFactory(IServiceProvider serviceProvider) : IOperationFactory
{
    public Dictionary<string, Operation> Operations => new (StringComparer.OrdinalIgnoreCase)
    {
        //Need to create new instance everytime, so GetOperation returns new instance everytime.
        { "output", serviceProvider.GetService<Output>() ?? throw new InvalidOperationException("Cannot resolve operation output")},
        { "setVariable", serviceProvider.GetService<SetVariable>() ?? throw new InvalidOperationException("Cannot resolve operation setVariable") },
        { "terminate", serviceProvider.GetService<Terminate>() ?? throw new InvalidOperationException("Cannot resolve operation terminate") },
        { "forEach", serviceProvider.GetService<ForEach>() ?? throw new InvalidOperationException("Cannot resolve operation forEach") },
        { "restCall", serviceProvider.GetService<RestCall>() ?? throw new InvalidOperationException("Cannot resolve operation restCall") }
    };
    
    //TODO: Add test that checks if every class derived from Operation is created here
    public Operation? GetOperation(string? name) =>
        !string.IsNullOrWhiteSpace(name) && Operations.TryGetValue(name, out var operation) ? operation : null;
}