namespace Models.Interfaces;

public interface IOperationFactory
{
    Operation? GetOperation(string? name);
}