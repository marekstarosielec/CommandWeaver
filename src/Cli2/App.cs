using CommandLine;
using Models.Interfaces.Context;

namespace Cli2;

// Application entry class
public class App(IContext context, Parser parser)
{
    public async Task Run(string[] args)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Token.ThrowIfCancellationRequested();

        Console.CancelKeyPress += (sender, args) => {
            args.Cancel = true; // Prevent default termination
            cancellationTokenSource.Cancel();       // Trigger cancellation
        };

        try
        {
            parser.ParseFullCommandLine(Environment.CommandLine, out var command, out var arguments);
            await context.Initialize(cancellationTokenSource.Token);
            await context.Run(command, arguments, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine("User abort...");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}