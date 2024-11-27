/// <inheritdoc />
public class CommandWeaver(
    ICommandService commandService,
    IFlowService flowService,
    ILoader loader,
    ISaver saver,
    IOutputService outputService,
    IOutputSettings outputSettings,
    ICommandParameterResolver commandParameterResolver,
    ICommandValidator commandValidator,
    IRepositoryElementStorage repositoryElementStorage) : ICommandWeaver
{
    /// <inheritdoc />
    public async Task Run(string commandName, Dictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        if (!HandleLogLevel(arguments)) return;
        
        if (string.IsNullOrWhiteSpace(commandName))
        {
            flowService.Terminate($"Command not provided.");
            return;
        }

        outputService.Trace($"Starting execution for command: {commandName}");

        await loader.Execute(cancellationToken);
        commandValidator.ValidateCommands(repositoryElementStorage.Get());

        var commandToExecute = commandService.Get(commandName);
        if (commandToExecute == null)
        {
            flowService.Terminate($"Unknown command {commandName}");
            return;
        }

        commandParameterResolver.PrepareCommandParameters(commandToExecute!, arguments);
        await commandService.ExecuteOperations(commandToExecute!.Operations, cancellationToken);
        await saver.Execute(cancellationToken);

        outputService.Trace($"Execution completed for command: {commandName}");
    }

    /// <summary>
    /// This handles log-level as first thing in execution flow, so only messages with correct log-level are displayed.
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    private bool HandleLogLevel(Dictionary<string, string> arguments)
    {
        if (arguments.TryGetValue("log-level", out var logLevelArgument))
        {
            if (!Enum.TryParse(logLevelArgument, true, out LogLevel logLevel))
            {
                flowService.Terminate(
                    $"Invalid value for argument \"log-level\". Allowed enum values: {string.Join(", ", Enum.GetNames(typeof(LogLevel)))}.");
                return false;
            }
            outputSettings.CurrentLogLevel = logLevel;
        }

        return true;
    }
}

