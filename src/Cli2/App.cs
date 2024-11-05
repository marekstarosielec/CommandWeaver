using CommandLine;
using Models.Interfaces.Context;

namespace Cli2;

// Application entry class
public class App(IContext context, Parser parser)
{
    public async Task Run(string[] args)
    {
        try
        {
            parser.ParseFullCommandLine(Environment.CommandLine, out var command, out var arguments);
            await context.Initialize(CancellationToken.None);
            await context.Run(command, arguments, CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}