// using System.Collections.Immutable;
// using System.Text;
// using System.Text.Json;
// using NSubstitute;
//
// public class OperationConverterTests
// {
//     private readonly IOperationFactory _operationFactory;
//     private readonly OperationConverter _converter;
//
//     public OperationConverterTests()
//     {
//         _operationFactory = Substitute.For<IOperationFactory>();
//         var outputService = Substitute.For<IOutputService>();
//         var variableService = Substitute.For<IVariableService>();
//         var flowService = Substitute.For<IFlowService>();
//         var conditionsService = Substitute.For<IConditionsService>();
//         _converter = new OperationConverter(
//             outputService,
//             variableService,
//             _operationFactory,
//             flowService,
//             conditionsService);
//     }
//
//     [Fact]
//     public void Read_ShouldReturnOperation_WhenValidJson()
//     {
//         // Arrange
//         var operationName = "TestOperation";
//         var testOperation = new TestOperation
//         {
//             Parameters = ImmutableDictionary<string, OperationParameter>.Empty
//                 .Add("param1", new OperationParameter { Description = "Test parameter" })
//         };
//
//         _operationFactory.GetOperation(operationName).Returns(testOperation);
//
//         var json = @"{
//             ""operation"": ""TestOperation"",
//             ""param1"": ""value1""
//         }";
//         var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
//
//         // Act
//         var result = _converter.Read(ref reader, typeof(Operation), null);
//
//         // Assert
//         Assert.NotNull(result);
//         Assert.IsType<TestOperation>(result);
//         Assert.Equal("value1", result.Parameters["param1"].OriginalValue.TextValue);
//     }
//
//     [Fact]
//     public void Read_ShouldThrow_WhenOperationNameIsMissing()
//     {
//         // Arrange
//         var json = @"{ ""param1"": ""value1"" }";
//         var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
//
//         // Act
//         Exception? exception = null;
//
//         try
//         {
//             _converter.Read(ref reader, typeof(Operation), null);
//         }
//         catch (Exception ex)
//         {
//             exception = ex;
//         }
//
//         // Assert
//         Assert.NotNull(exception);
//         Assert.Contains("Operation name cannot be null or empty.", exception.Message);
//     }
//     
//     [Fact]
//     public void Read_ShouldThrow_WhenOperationIsUnknown()
//     {
//         // Arrange
//         var operationName = "UnknownOperation";
//         _operationFactory.GetOperation(operationName).Returns((Operation)null!);
//     
//         var json = @"{ ""operation"": ""UnknownOperation"" }";
//         var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
//     
//         // Act
//         Exception? exception = null;
//
//         try
//         {
//             _converter.Read(ref reader, typeof(Operation), null);
//         }
//         catch (Exception ex)
//         {
//             exception = ex;
//         }
//     
//         // Assert
//         Assert.NotNull(exception);
//         Assert.Contains("could not be resolved.", exception.Message);
//     }
//
//     [Fact]
//     public void Read_ShouldConfigureSubOperations_WhenAggregateOperation()
//     {
//         // Arrange
//         var parentOperation = new TestAggregateOperation
//         {
//             Parameters = ImmutableDictionary<string, OperationParameter>.Empty
//         };
//         var subOperation = new TestOperation
//         {
//             Parameters = ImmutableDictionary<string, OperationParameter>.Empty
//         };
//
//         _operationFactory.GetOperation("ParentOperation").Returns(parentOperation);
//         _operationFactory.GetOperation("SubOperation").Returns(subOperation);
//
//         var json = @"{
//             ""operation"": ""ParentOperation"",
//             ""operations"": [
//                 { ""operation"": ""SubOperation"" }
//             ]
//         }";
//         var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
//
//         // Act
//         var result = _converter.Read(ref reader, typeof(Operation), null);
//
//         // Assert
//         Assert.NotNull(result);
//         Assert.IsType<TestAggregateOperation>(result);
//         var aggregate = (TestAggregateOperation)result;
//         Assert.Single(aggregate.Operations);
//         Assert.IsType<TestOperation>(aggregate.Operations[0]);
//     }
//
//     [Fact]
//     public void ConfigureParameter_ShouldSetCorrectParameterValues()
//     {
//         // Arrange
//         var operation = new TestOperation
//         {
//             Parameters = ImmutableDictionary<string, OperationParameter>.Empty
//                 .Add("param1", new OperationParameter { Description = "Test parameter" })
//         };
//
//         var json = @"{
//             ""operation"": ""TestOperation"",
//             ""param1"": ""value1""
//         }";
//         var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
//         _operationFactory.GetOperation("TestOperation").Returns(operation);
//
//         // Act
//         var result = _converter.Read(ref reader, typeof(Operation), null);
//
//         // Assert
//         Assert.NotNull(result);
//         Assert.Equal("value1", result.Parameters["param1"].OriginalValue.TextValue);
//     }
// }
