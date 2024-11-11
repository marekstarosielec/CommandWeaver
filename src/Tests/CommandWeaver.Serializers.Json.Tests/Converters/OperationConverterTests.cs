using System.Text.Json;
using NSubstitute;

public class OperationConverterTests
{
    private readonly IOutput _mockOutput;
    private readonly IVariables _variables;
    private readonly IOperationFactory _mockFactory;
    private readonly JsonSerializerOptions _options;

    public OperationConverterTests()
    {
        _mockOutput = Substitute.For<IOutput>();
        _mockFactory = Substitute.For<IOperationFactory>();
        _variables = Substitute.For<IVariables>();
        _variables.CurrentlyLoadRepositoryElement.Returns("TestElement");

        _options = new JsonSerializerOptions
        {
            Converters = { new ConverterWrapper<Operation>(new OperationConverter(_mockOutput, _variables,  _mockFactory)) }
        };
    }

    [Fact]
    public void Read_MissingOperationName_ShouldReturnNull()
    {
        string json = "{\"parameter1\": \"value1\"}";

        var result = JsonSerializer.Deserialize<Operation>(json, _options);

        Assert.Null(result);
        _mockOutput.Received(1).Warning(Arg.Is<string>(msg => msg.Contains("TestElement")));
    }

    [Fact]
    public void Read_InvalidOperationName_ShouldReturnNull()
    {
        string json = "{\"operation\": \"invalidOperation\"}";

        _mockFactory.GetOperation("invalidOperation").Returns((Operation)null);

        var result = JsonSerializer.Deserialize<Operation>(json, _options);

        Assert.Null(result);
        _mockOutput.Received(1).Warning(
            Arg.Is<string>(msg => msg.Contains("invalidOperation") && msg.Contains("TestElement")));
    }

    [Fact]
    public void Read_ValidOperation_ShouldPopulateParameters()
    {
        string json = "{\"operation\": \"testOperation\", \"parameter1\": \"value1\", \"parameter2\": 42}";

        var testOperation = new TestOperation();
        testOperation.Parameters["parameter1"] = new OperationParameter { Description = "description" };
        testOperation.Parameters["parameter2"] = new OperationParameter { Description = "description" };

        _mockFactory.GetOperation("testOperation").Returns(testOperation);

        var result = JsonSerializer.Deserialize<Operation>(json, _options);

        Assert.NotNull(result);
        Assert.Equal("value1", result.Parameters["parameter1"]?.Value.TextValue);
        Assert.Equal(42, result.Parameters["parameter2"]?.Value.NumericValue);
    }

    [Fact]
    public void Read_InvalidParameter_ShouldLogWarningAndContinue()
    {
        string json = "{\"operation\": \"testOperation\", \"invalidParameter\": \"value1\"}";

        var testOperation = new TestOperation();

        _mockFactory.GetOperation("testOperation").Returns(testOperation);

        var result = JsonSerializer.Deserialize<Operation>(json, _options);

        Assert.NotNull(result);
        _mockOutput.Received(1).Warning(
            Arg.Is<string>(msg => msg.Contains("invalidParameter") && msg.Contains("testOperation") && msg.Contains("TestElement")));
    }

    [Fact]
    public void Read_Conditions_ShouldSetCorrectly()
    {
        string json = "{\"operation\": \"testOperation\", \"conditions\": { \"IsNull\": true, \"IsNotNull\": false }}";

        var testOperation = new TestOperation();

        _mockFactory.GetOperation("testOperation").Returns(testOperation);

        var result = JsonSerializer.Deserialize<Operation>(json, _options);

        Assert.NotNull(result);
        Assert.True(result.Conditions.IsNull?.BoolValue);
        Assert.False(result.Conditions.IsNotNull?.BoolValue);
    }
}
