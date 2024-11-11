/// <summary>
/// Main CommandWeaver entry point. Orchestrates all flow.
/// </summary>
public interface ICommandWeaver
{
    /// <summary>
    /// Main CommandWeaver entry point. Loads commands, variables, runs commands and saves changes.
    /// </summary>
    /// <param name="commmandName">Name of command. It can consist of multiple words.</param>
    /// <param name="arguments">Arguments for command.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Run(string commmandName, Dictionary<string, string> arguments, CancellationToken cancellationToken);
}