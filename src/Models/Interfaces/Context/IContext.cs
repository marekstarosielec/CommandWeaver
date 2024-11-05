namespace Models.Interfaces.Context;

public interface IContext
{
    IContextServices Services { get; }
    IContextVariables Variables { get; }
    
    Task Initialize(CancellationToken cancellationToken);
    Task Run(string commmandName, Dictionary<string, string> arguments, CancellationToken cancellationToken = default);
    void Terminate(string? message = null, int exitCode = 1);
}