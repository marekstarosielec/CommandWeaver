using Models;
using Models.Interfaces;

namespace BuiltInOperations;

public class OperationFactory : IOperationFactory
{
    public Operation? GetOperation(string? name) =>
        name?.ToLower() switch
        {
            "output" => new OutputOperation(),
            "setvariable" => new SetVariable(),
            _ => null
        };
}