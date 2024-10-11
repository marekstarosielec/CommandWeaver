// using BuiltInOperations;
//
// namespace Serializer.Json.Tests;
//
// public class JsonSerializerTests
// {
//     [Fact]
//     public void Serializer_ReturnsInt_WhenIntInJson()
//     {
//         var json = "{\"abc\":123}";
//         var sut = new JsonSerializer(new OperationConverter(new OperationFactory()));
//         var result = sut.Deserialize<Dictionary<string, object?>>(json);
//         Assert.Equal(123, result!["abc"]);
//     }
//     
//     [Fact]
//     public void Serializer_ReturnsString_WhenStringInJson()
//     {
//         var json = "{\"abc\":\"123\"}";
//         var sut = new JsonSerializer(new OperationConverter(new OperationFactory()));
//         var result = sut.Deserialize<Dictionary<string, object?>>(json);
//         Assert.Equal("123", result!["abc"]);
//     }
//     
//     [Fact]
//     public void Serializer_ReturnsBool_WhenBoolInJson()
//     {
//         var json = "{\"abc\":true}";
//         var sut = new JsonSerializer(new OperationConverter(new OperationFactory()));
//         var result = sut.Deserialize<Dictionary<string, object?>>(json);
//         Assert.True(result!["abc"] as bool?);
//     }
//     
//     [Fact]
//     public void Serializer_ReturnsDecimal_WhenDecimalInJson()
//     {
//         var json = "{\"abc\":1.23}";
//         var sut = new JsonSerializer(new OperationConverter(new OperationFactory()));
//         var result = sut.Deserialize<Dictionary<string, object?>>(json);
//         Assert.Equal(1.23, result!["abc"]);
//     }
// }