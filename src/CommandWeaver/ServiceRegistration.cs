using Microsoft.Extensions.DependencyInjection;
using SpectreConsole;

public static class ServiceRegistration
{
    public static void AddCommandWeaver(this IServiceCollection services)
    {
        services.AddTransient<ILoader, Loader>();
        services.AddTransient<ISaver, Saver>();
        services.AddSingleton<IRepositoryElementStorage, RepositoryElementStorage>(); //Needs to be singleton, because it stores data.
        services.AddTransient<ICommandWeaver, CommandWeaver>();
        services.AddTransient<IOutputWriter, SpectreConsoleOutput>();
        services.AddCommandWeaverSerialization();
        services.AddCommandWeaverFlow();
        services.AddCommandWeaverOutput();
        services.AddCommandWeaverVariables();
        services.AddCommandWeaverFileRepository();
        services.AddCommandWeaverEmbeddedRepository();
        services.AddCommandWeaverCommands();
    }
}
