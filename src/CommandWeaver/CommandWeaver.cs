/// <inheritdoc />
public class CommandWeaver(
    ICommandService commandService,
    ILoader loader,
    ISaver saver,
    IOutputService outputService,
    IOutputSettings outputSettings,
    ICommandParameterResolver commandParameterResolver,
    ICommandValidator commandValidator,
    IRepositoryElementStorage repositoryElementStorage,
    IBackgroundService backgroundService) : ICommandWeaver
{
    /// <inheritdoc />
    public async Task Run(string commandName, Dictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        try
        {
            HandleLogLevel(arguments);
        
            if (string.IsNullOrWhiteSpace(commandName))
                throw new CommandWeaverException($"Command not provided.");

            outputService.Trace($"Starting execution for command: {commandName}");

            await loader.Execute(cancellationToken);
            commandValidator.ValidateCommands(repositoryElementStorage.Get());

            var commandToExecute = commandService.Get(commandName);
            if (commandToExecute == null)
                throw new CommandWeaverException($"Unknown command {commandName}");

            commandParameterResolver.PrepareCommandParameters(commandToExecute!, arguments);
            await commandService.ExecuteOperations(commandToExecute!.Operations, cancellationToken);
            await saver.Execute(cancellationToken);
            outputService.Trace($"Awaiting background tasks to complete");
            await backgroundService.WaitToComplete();
            outputService.Trace($"Execution completed for command: {commandName}");
        }
        catch (OperationCanceledException)
        {
            outputService.Information("User abort...");
        }
        catch (CommandWeaverException e)
        {
            outputService.Error(e.Message);
            throw;
        }
        finally
        {
            backgroundService.Stop();
        }
    }

    /// <summary>
    /// This handles log-level as first thing in execution flow, so only messages with correct log-level are displayed.
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    private void HandleLogLevel(Dictionary<string, string> arguments)
    {
        if (!arguments.TryGetValue("log-level", out var logLevelArgument)) return;
        if (Enum.TryParse(logLevelArgument, true, out LogLevel logLevel))
            outputSettings.CurrentLogLevel = logLevel;
        else
            throw new CommandWeaverException(
                $"Invalid value for argument \"log-level\". Allowed enum values: {string.Join(", ", Enum.GetNames(typeof(LogLevel)))}.");
    }
}

