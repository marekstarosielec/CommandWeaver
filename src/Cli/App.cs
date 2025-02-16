// Application entry class
public class App(ICommandWeaver commandWeaver, Parser parser)
{
    public async Task Run(string[] args)
    {
        /*
         TODO: Move test commands out of embedded.
         TODO: Add event support to rest operation.
         TODO: add browser support.
         TODO: Add help command about single command.
         TODO: start rest server and add events.
         TODO: change internal logging level - everything to Trace, operations to Debug
         
            TODO: Support for prompts.
            TODO: Add a command for setting a new log level.
            TODO: Add encryption support for variables.
            TODO: Add multiline argument support.
            
        */
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Token.ThrowIfCancellationRequested();

        Console.CancelKeyPress += (_, cancelArgs) => {
            cancelArgs.Cancel = true; // Prevent default termination
            cancellationTokenSource.Cancel();       // Trigger cancellation
        };

        try
        {
            parser.ParseFullCommandLine(Environment.CommandLine, out var command, out var arguments);
            await commandWeaver.Run(command, arguments, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("User abort...");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
    }
}