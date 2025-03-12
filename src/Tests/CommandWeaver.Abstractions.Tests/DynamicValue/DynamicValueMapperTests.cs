public class DynamicValueMapperTests
{
    [Fact]
    public void MapTo_ShouldMapCorrectly_ForTopLevelDynamicValue()
    {
        // Arrange
        var input = new DynamicValue("Abc");

        // Act
        var result = DynamicValueMapper.MapTo<DynamicValue>(input);

        // Assert
        Assert.Equal("Abc", result?.TextValue);
    }
    
    [Fact]
    public void MapTo_ShouldMapCorrectly_ForTopLevelString()
    {
        // Arrange
        var input = new DynamicValue("Abc");

        // Act
        var result = DynamicValueMapper.MapTo<string>(input);

        // Assert
        Assert.Equal("Abc", result);
    }
    
    [Fact]
    public void MapTo_ShouldMapCorrectly_ForTopLevelStringWhenTargetIsList()
    {
        // Arrange
        var input = new DynamicValue("Abc");

        // Act
        var result = DynamicValueMapper.MapTo<List<string>>(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Abc", result.FirstOrDefault());
        Assert.Single(result!);
    }
    
    [Fact]
    public void MapTo_ShouldFail_WhenDynamicValueContainsPropertyWhichDoesNotExistInTargetType()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "Name", new DynamicValue("John Doe") },
            { "Age", new DynamicValue(30) },
            { "DoesNotExist", new DynamicValue(30) }
        });
    
        // Act
        Assert.Throws<InvalidOperationException>(() => DynamicValueMapper.MapTo<Person>(dynamicValue));
    }
    
    [Fact]
    public void MapTo_SimpleProperties_ShouldMapCorrectly()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "Name", new DynamicValue("John Doe") },
      //      { "Age", new DynamicValue(30) }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.Equal("John Doe", person?.Name);
     //   Assert.Equal(30, person?.Age);
    }
    //
    // [Fact]
    // public void MapTo_NestedProperties_ShouldMapCorrectly()
    // {
    //     // Arrange
    //     var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
    //     {
    //         { "Name", new DynamicValue("John Doe") },
    //         { "Age", new DynamicValue(30) },
    //         { "HomeAddress", new DynamicValue(new Dictionary<string, DynamicValue?>
    //             {
    //                 { "Street", new DynamicValue("123 Main St") },
    //                 { "City", new DynamicValue("Metropolis") }
    //             })
    //         }
    //     });
    //
    //     // Act
    //     var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    //
    //     // Assert
    //     Assert.Equal("John Doe", person?.Name);
    //     Assert.Equal(30, person?.Age);
    //     Assert.NotNull(person?.HomeAddress);
    //     Assert.Equal("123 Main St", person?.HomeAddress?.Street);
    //     Assert.Equal("Metropolis", person?.HomeAddress?.City);
    // }
    //
    // [Fact]
    // public void MapTo_MissingProperties_ShouldIgnoreMissing()
    // {
    //     // Arrange
    //     var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
    //     {
    //         { "Name", new DynamicValue("Jane Doe") }
    //     });
    //
    //     // Act
    //     var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    //
    //     // Assert
    //     Assert.Equal("Jane Doe", person?.Name);
    //     Assert.Equal(0, person?.Age); // Default value for int
    //     Assert.Null(person?.HomeAddress); // Not set
    // }
    //
    // [Fact]
    // public void MapTo_InvalidDynamicValue_ShouldReturnNull()
    // {
    //     // Arrange
    //     var invalidDynamicValue = new DynamicValue("InvalidValue");
    //
    //     // Act
    //     var result = DynamicValueMapper.MapTo<Person>(invalidDynamicValue);
    //     
    //     //Assert
    //     Assert.Null(result);
    // }
    //
    // [Fact]
    // public void MapTo_EmptyDynamicValue_ShouldReturnDefaultValues()
    // {
    //     // Arrange
    //     var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>());
    //
    //     // Act
    //     var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    //
    //     // Assert
    //     Assert.Equal(string.Empty, person?.Name);
    //     Assert.Equal(0, person?.Age);
    //     Assert.Null(person?.HomeAddress);
    // }
    //
    // [Fact]
    // public void MapTo_DeeplyNestedProperties_ShouldMapCorrectly()
    // {
    //     // Arrange
    //     var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
    //     {
    //         { "HomeAddress", new DynamicValue(new Dictionary<string, DynamicValue?>
    //             {
    //                 { "Street", new DynamicValue("123 Main St") },
    //                 { "City", new DynamicValue("Metropolis") }
    //             })
    //         }
    //     });
    //
    //     // Act
    //     var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    //
    //     // Assert
    //     Assert.NotNull(person?.HomeAddress);
    //     Assert.Equal("123 Main St", person?.HomeAddress?.Street);
    //     Assert.Equal("Metropolis", person?.HomeAddress?.City);
    // }
    
    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public Address? HomeAddress { get; set; }
    }

    internal class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
