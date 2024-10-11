using Models.Interfaces.Context;

namespace Cli2Context;

public class ContextServices(IOutput output) : IContextServices
{
    public IOutput Output { get; } = output;
}