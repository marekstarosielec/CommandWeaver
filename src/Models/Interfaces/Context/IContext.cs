namespace Models.Interfaces.Context;

public interface IContext
{

    
    IContextServices Services { get; }
    IContextVariables Variables { get; }
    
    Task Load(CancellationToken cancellationToken);
    Task Run(string commandLineArguments, CancellationToken cancellationToken);
}