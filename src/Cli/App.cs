// Application entry class
public class App(ICommandWeaver commandWeaver, IBackgroundService backgroundService, Parser parser)
{
    public async Task<int> Run(string[] args)
    {
        /*
            TODO: DefaultValue for operation parameter should be DynamicValue and should be filled automatically when evaluating, instead on in GetEnumValue
            TODO: add browser support.
            TODO: start rest server and add events.
            TODO: change internal logging level - everything to Trace, operations to Debug     
            TODO: Support for prompts.
            TODO: Add a command for setting a new log level.
            TODO: Add encryption support for variables.
            TODO: Add multiline argument support.
            TODO: Review all classes - refactor, check coverage.
            TODO: make sure that cancellationToken is used everywhere it can.
            IDEA: Refactor Operation Parameters to always return 1 parameter of type defined in operation (each operation has own type). Map Dynamic value of whole operation to that type. Include validation, description, etc in every property.
        */
        
        var cancellationTokenSource = new CancellationTokenSource();
        
        Console.CancelKeyPress += (_, cancelArgs) => {
            cancelArgs.Cancel = true; // Prevent default termination
            cancellationTokenSource.Cancel();       // Trigger cancellation
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
        {
            cancellationTokenSource.Cancel();
        };

        try
        {
            parser.ParseFullCommandLine(Environment.CommandLine, out var command, out var arguments);
            await commandWeaver.Run(command, arguments, cancellationTokenSource.Token);
        }
        catch (CommandWeaverException e)
        {
            return e.ExitCode;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            backgroundService.Stop();
        }

        return 0;
    }
}