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
    public void AllOperations_ShouldBePresentInFactory()
    {
        // Arrange
        var serviceProvider = CreateMockServiceProvider();
        var factory = new OperationFactory(serviceProvider);

        // Act
        var operationTypes = GetAllOperationTypes().ToList();
        if (!operationTypes.Any())
            Assert.Fail("OperationTypes list is empty");
        var operations = factory.GetOperations();

        // Assert
        foreach (var operationType in operationTypes.Where(operationType => !operations.Keys.Any(op => string.Equals(op, operationType.Name, StringComparison.OrdinalIgnoreCase))))
            Assert.Fail($"{operationType} is not registered in {nameof(OperationFactory)}");
    }

    private static IEnumerable<Type> GetAllOperationTypes() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(Operation).IsAssignableFrom(t));

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
            .Returns(new RestCall(conditionsService, variableService, jsonSerializer, flowService, outputService));
        serviceProvider.GetService(typeof(Block))
            .Returns(new Block(Substitute.For<ICommandService>()));
        serviceProvider.GetService(typeof(ListGroup))
            .Returns(new ListGroup(variableService));

        return serviceProvider;
    }
}
