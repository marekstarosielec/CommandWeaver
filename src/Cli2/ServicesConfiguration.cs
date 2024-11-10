using BuiltInOperations;
using Cli2Context;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Repositories.File;
using SpectreConsole;

namespace Cli2;

internal static class ServicesConfiguration
{
    internal static void Install(IServiceCollection services)
    {
        // Register services
        services.AddCommandWeaverSerialization();
        services.AddCommandWeaverFlowService();
        services.AddCommandWeaverVariables();


        services.AddTransient<IRepository, FileRepository>();
        services.AddTransient<IOutput, SpectreConsoleOutput>();
        services.AddSingleton<IContext, Context>(); //Only one context in all app.
        services.AddTransient<IOperationFactory, OperationFactory>();
         services.AddTransient<Parser>();
        // Register the main application entry point
        services.AddTransient<App>();
    }
}
