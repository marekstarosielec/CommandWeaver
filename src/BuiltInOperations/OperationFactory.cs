namespace BuiltInOperations;

public class OperationFactory : IOperationFactory
{
    //Add test that checks if every class derived from Operation is created here
    public Operation? GetOperation(string? name) =>
        name?.ToLower() switch
        {
            "output" => new Output(),
            "setvariable" => new SetVariable(),
            "terminate" => new Terminate(),
            _ => null
        };
}