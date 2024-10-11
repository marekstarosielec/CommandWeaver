using BuiltInOperations;
using Cli2Context;
using Microsoft.Extensions.DependencyInjection;
using Models.Interfaces;
using Models.Interfaces.Context;
using Repositories.Abstraction.Interfaces;
using Repositories.File;
using Serializer.Abstractions;
using Serializer.Json;
using SpectreConsole;

// ReSharper disable ClassNeverInstantiated.Global

namespace Cli2;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup Dependency Injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        // Build ServiceProvider
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Resolve the service and use it
        var app = serviceProvider.GetService<App>();
        await app?.Run(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register services
        services.AddTransient<JsonSerializer>();
        services.AddTransient<ISerializerFactory, SerializerFactory>();
        services.AddTransient<IRepository, FileRepository>();
        services.AddTransient<IOutput, SpectreConsoleOutput>();
        services.AddSingleton<IContext, Context>(); //Only one context in all app.
        services.AddTransient<IOperationFactory, OperationFactory>();
        services.AddTransient<OperationConverter>();
        
        // Register the main application entry point
        services.AddTransient<App>();
    }
}