using BuiltInOperations;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using SpectreConsole;

namespace Cli2;

internal static class ServicesConfiguration
{
    internal static void Install(IServiceCollection services)
    {
        // Register services
        services.AddCommandWeaver();

        services.AddTransient<IOperationFactory, OperationFactory>();
         services.AddTransient<Parser>();
        // Register the main application entry point
        services.AddTransient<App>();
    }
}
