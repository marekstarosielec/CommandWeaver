using Microsoft.Extensions.DependencyInjection;

// Setup Dependency Injection
var serviceCollection = new ServiceCollection();
ServicesConfiguration.Install(serviceCollection);

// Build ServiceProvider
var serviceProvider = serviceCollection.BuildServiceProvider();

// Resolve the service and use it
var app = serviceProvider.GetService<App>();
if (app != null)
{
    await app.Run(args);
    return;
}

Console.WriteLine("Fatal error: failed to initialize application");
Environment.Exit(1);