// Application entry class
public class App(ICommandWeaver commandWeaver, Parser parser)
{
    public async Task Run(string[] args)
    {
        /*
            Support for prompts.
            Support for sessions.
            Add a command for setting a new log level.
            Add encryption support for variables.
            Add multiline argument support.
            
            readme:
            general description
            commands
            operations
            styling tags
            variables + syntax + encryption
            rest command 
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