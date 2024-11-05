using BuiltInOperations;
using Cli2Context;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Models.Interfaces.Context;
using Models.Interfaces;
using Repositories.Abstraction.Interfaces;
using Repositories.File;
using Serializer.Abstractions;
using Serializer.Json;
using SpectreConsole;

namespace Cli2;

internal static class ServicesConfiguration
{
    internal static void Install(IServiceCollection services)
    {
        // Register services
        services.AddTransient<JsonSerializer>();
        services.AddTransient<ISerializerFactory, SerializerFactory>();
        services.AddTransient<IRepository, FileRepository>();
        services.AddTransient<IOutput, SpectreConsoleOutput>();
        services.AddSingleton<IContext, Context>(); //Only one context in all app.
        services.AddTransient<IOperationFactory, OperationFactory>();
        services.AddTransient<OperationConverter>();
        services.AddTransient<Parser>();
        // Register the main application entry point
        services.AddTransient<App>();
    }
}
