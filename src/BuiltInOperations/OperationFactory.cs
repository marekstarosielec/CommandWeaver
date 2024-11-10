namespace BuiltInOperations;

public class OperationFactory(IOutput output, IVariables variables, IFlow flow) : IOperationFactory
{
    //Add test that checks if every class derived from Operation is created here
    public Operation? GetOperation(string? name) =>
        name?.ToLower() switch
        {
            //TODO: Make operations injectable
            "output" => new Output(output),
            "setvariable" => new SetVariable(variables),
            "terminate" => new Terminate(flow),
            _ => null
        };
}