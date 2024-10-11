using Cli2Context;
using Models.Interfaces;
using Models.Interfaces.Context;

namespace Cli2;

// Application entry class
public class App(IContext context)
{
    public async Task Run(string[] args)
    {
        try
        {
            await context.Load(CancellationToken.None);
            await context.Run(Environment.CommandLine, CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}