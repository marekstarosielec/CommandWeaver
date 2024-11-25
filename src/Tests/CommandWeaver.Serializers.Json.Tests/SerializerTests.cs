// using NSubstitute;
// using System.Text.RegularExpressions;
//
// public class SerializerTests
// {
//     private readonly JsonSerializer _serializer;
//
//     public SerializerTests()
//     {
//         // Substitute custom converters
//         var operationConverter = Substitute.For<IOperationConverter>();
//         var dynamicValueConverter = Substitute.For<IDynamicValueConverter>();
//
//         // Initialize JsonSerializer with custom converters
//         _serializer = new JsonSerializer(operationConverter, dynamicValueConverter);
//     }
//
//     [Fact]
//     public void TryDeserialize_ValidJson_ShouldReturnTrueAndPopulateResult()
//     {
//         string json = "{\"name\": \"example\"}";
//
//         bool success = _serializer.TryDeserialize<MyClass>(json, out var result, out var exception);
//
//         Assert.True(success);
//         Assert.NotNull(result);
//         Assert.Equal("example", result?.Name);
//         Assert.Null(exception);
//     }
//
//     [Fact]
//     public void TryDeserialize_InvalidJson_ShouldReturnFalseAndPopulateException()
//     {
//         string invalidJson = "{\"name\": example}";
//
//         bool success = _serializer.TryDeserialize<MyClass>(invalidJson, out var result, out var exception);
//
//         Assert.False(success);
//         Assert.Null(result);
//         Assert.NotNull(exception);
//     }
//
//     [Fact]
//     public void TrySerialize_ValidObject_ShouldReturnTrueAndPopulateResult()
//     {
//         var myObject = new MyClass { Name = "example" };
//
//         bool success = _serializer.TrySerialize(myObject, out var result, out var exception);
//
//         Assert.True(success);
//         Assert.NotNull(result);
//         Assert.Matches(new Regex("\"name\"\\s*:\\s*\"example\"", RegexOptions.Compiled), result!);
//         Assert.Null(exception);
//     }
//
//     [Fact]
//     public void TrySerialize_ObjectWithNullProperties_ShouldIgnoreNulls()
//     {
//         var myObject = new MyClass { Name = null };
//
//         bool success = _serializer.TrySerialize(myObject, out var result, out var exception);
//
//         Assert.True(success);
//         Assert.NotNull(result);
//         Assert.DoesNotContain("\"name\"", result);
//         Assert.Null(exception);
//     }
//
//     [Fact]
//     public void TrySerialize_InvalidObject_ShouldReturnFalseAndPopulateException()
//     {
//         var invalidObject = new NonSerializableClass();
//
//         bool success = _serializer.TrySerialize(invalidObject, out var result, out var exception);
//
//         Assert.False(success);
//         Assert.Null(result);
//         Assert.NotNull(exception);
//     }
// }
//
// // Sample classes used for testing
// public class MyClass
// {
//     public string? Name { get; set; }
// }
//
// public class NonSerializableClass
// {
//     public IntPtr Handle { get; set; }  // IntPtr is not serializable by default
// }
