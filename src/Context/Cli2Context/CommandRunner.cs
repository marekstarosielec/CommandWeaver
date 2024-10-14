using Models;
using Models.Interfaces;
using Models.Interfaces.Context;

namespace Cli2Context;

public class CommandRunner
{
    public async Task RunCommand(Command command, IContext context, CancellationToken cancellationToken)
    {
        foreach (var operation in command.Operations)
        {
            await operation.Run(context, cancellationToken);
        }
    }
}