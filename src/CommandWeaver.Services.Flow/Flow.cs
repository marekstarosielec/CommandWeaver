/// <inheritdoc />
public class Flow(IOutput output, IOperationConditions operationConditions) : IFlow
{
    public async Task ExecuteCommand(Command command, IVariables variables, CancellationToken cancellationToken)
    {
        foreach (var operation in command.Operations)
            if (!operationConditions.OperationShouldBeSkipped(operation, variables) 
                && !cancellationToken.IsCancellationRequested)
                await ExecuteOperation(operation, variables, cancellationToken);
    }

    internal async Task ExecuteOperation(Operation operation, IVariables variables, CancellationToken cancellationToken)
    {
        PrepareOperationParameters(operation, variables);
        await operation.Run(cancellationToken);
    }

    /// <inheritdoc />
    public void Terminate(string? message = null, int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(message))
            output.Error(message);
        Environment.Exit(exitCode);
    }

    private void PrepareOperationParameters(Operation operation, IVariables variables)
    {
        foreach (var parameterKey in operation.Parameters.Keys)
        {
            //Evaluate all operation parametes.
            operation.Parameters[parameterKey] = operation.Parameters[parameterKey] with { Value = variables.ReadVariableValue(operation.Parameters[parameterKey].Value) ?? new DynamicValue() };
            if (operation.Parameters[parameterKey].Required && operation.Parameters[parameterKey].Value.IsNull())
                Terminate($"Parameter {parameterKey} is required in operation {operation.Name}.");
            if (operation.Parameters[parameterKey].RequiredText && string.IsNullOrWhiteSpace(operation.Parameters[parameterKey].Value.TextValue))
                Terminate($"Parameter {parameterKey} requires text value in operation {operation.Name}.");

            if (operation.Parameters[parameterKey].AllowedEnumValues != null)
            {
                //TODO: This should be checked by unit test.
                if (!operation.Parameters[parameterKey].AllowedEnumValues!.IsEnum)
                    Terminate($"Parameter {parameterKey} contains invalid AllowedEnumValues in operation {operation.Name}.");

                if (!string.IsNullOrWhiteSpace(operation.Parameters[parameterKey].Value.TextValue) && !Enum.IsDefined(operation.Parameters[parameterKey].AllowedEnumValues!, operation.Parameters[parameterKey].Value.TextValue!))
                    Terminate($"Parameter {parameterKey} has invalid value in operation {operation.Name}.");
            }
        }
    }
}
