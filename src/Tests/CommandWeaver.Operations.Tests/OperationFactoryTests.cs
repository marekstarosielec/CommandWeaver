using NSubstitute;
using System.Reflection;

public class OperationFactoryTests
{
    [Fact]
    public void GetOperation_ShouldReturnCorrectInstance()
    {
        // Arrange
        var serviceProvider = CreateMockServiceProvider();
        var factory = new OperationFactory(serviceProvider);

        // Act
        var outputOperation = factory.GetOperation("output");
        var setVariableOperation = factory.GetOperation("setVariable");
        var terminateOperation = factory.GetOperation("terminate");
        var forEachOperation = factory.GetOperation("forEach");
        var restCallOperation = factory.GetOperation("restCall");

        // Assert
        Assert.NotNull(outputOperation);
        Assert.NotNull(setVariableOperation);
        Assert.NotNull(terminateOperation);
        Assert.NotNull(forEachOperation);
        Assert.NotNull(restCallOperation);
        Assert.IsType<Output>(outputOperation);
        Assert.IsType<SetVariable>(setVariableOperation);
        Assert.IsType<Terminate>(terminateOperation);
        Assert.IsType<ForEach>(forEachOperation);
        Assert.IsType<RestCall>(restCallOperation);
    }

    [Fact]
    public void GetOperation_ShouldReturnNullForUnknownOperation()
    {
        // Arrange
        var serviceProvider = CreateMockServiceProvider();
        var factory = new OperationFactory(serviceProvider);

        // Act
        var operation = factory.GetOperation("unknown");

        // Assert
        Assert.Null(operation);
    }

    [Fact]
    public void GetOperations_ShouldReturnAllOperations()
    {
        // Arrange
        var serviceProvider = CreateMockServiceProvider();
        var factory = new OperationFactory(serviceProvider);

        // Act
        var operations = factory.GetOperations();

        // Assert
        Assert.Equal(5, operations.Count);
        Assert.Contains("output", operations.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("setVariable", operations.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("terminate", operations.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("forEach", operations.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("restCall", operations.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void AllOperations_ShouldBePresentInFactory()
    {
        // Arrange
        var serviceProvider = CreateMockServiceProvider();
        var factory = new OperationFactory(serviceProvider);

        // Act
        var operationTypes = GetAllOperationTypes();
        var operations = factory.GetOperations();

        // Assert
        foreach (var operationType in operationTypes)
            Assert.Contains(operations.Values, op => op.GetType() == operationType);
    }

    private static IEnumerable<Type> GetAllOperationTypes()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(Operation).IsAssignableFrom(t));
    }

    private static IServiceProvider CreateMockServiceProvider()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        var variableService = Substitute.For<IVariableService>();
        var flowService = Substitute.For<IFlowService>();
        var conditionsService = Substitute.For<IConditionsService>();
        var jsonSerializer = Substitute.For<IJsonSerializer>();
        var outputService = Substitute.For<IOutputService>();

        // Mock the IServiceProvider.GetService behavior
        serviceProvider.GetService(typeof(Output))
            .Returns(new Output(outputService));
        serviceProvider.GetService(typeof(SetVariable))
            .Returns(new SetVariable(variableService));
        serviceProvider.GetService(typeof(Terminate))
            .Returns(new Terminate(flowService));
        serviceProvider.GetService(typeof(ForEach))
            .Returns(new ForEach(Substitute.For<ICommandService>(), variableService, outputService));
        serviceProvider.GetService(typeof(RestCall))
            .Returns(new RestCall(conditionsService, variableService, jsonSerializer));

        return serviceProvider;
    }
}
