// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
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
        Assert.Single(result);
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
            { "Age", new DynamicValue(30) }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.Equal("John Doe", person?.Name);
        Assert.Equal(30, person?.Age);
    }
    
    [Fact]
    public void MapTo_NestedProperties_ShouldMapCorrectly()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "Name", new DynamicValue("John Doe") },
            { "Age", new DynamicValue(30) },
            { "HomeAddress", new DynamicValue(new Dictionary<string, DynamicValue?>
                {
                    { "Street", new DynamicValue("123 Main St") },
                    { "City", new DynamicValue("Metropolis") }
                })
            }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.Equal("John Doe", person?.Name);
        Assert.Equal(30, person?.Age);
        Assert.NotNull(person?.HomeAddress);
        Assert.Equal("123 Main St", person.HomeAddress.Street);
        Assert.Equal("Metropolis", person.HomeAddress.City);
    }
    
    [Fact]
    public void MapTo_MissingProperties_ShouldIgnoreMissing()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "Name", new DynamicValue("Jane Doe") }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.Equal("Jane Doe", person?.Name);
        Assert.Equal(0, person?.Age); // Default value for int
        Assert.Null(person?.HomeAddress); // Not set
    }
    
    [Fact]
    public void MapTo_InvalidDynamicValue_ShouldReturnNull()
    {
        // Arrange
        var invalidDynamicValue = new DynamicValue("InvalidValue");
    
        // Act
        var result = DynamicValueMapper.MapTo<Person>(invalidDynamicValue);
        
        //Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void MapTo_EmptyDynamicValue_ShouldReturnDefaultValues()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>());
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.Equal(string.Empty, person?.Name);
        Assert.Equal(0, person?.Age);
        Assert.Null(person?.HomeAddress);
    }
    
    [Fact]
    public void MapTo_DeeplyNestedProperties_ShouldMapCorrectly()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "HomeAddress", new DynamicValue(new Dictionary<string, DynamicValue?>
                {
                    { "Street", new DynamicValue("123 Main St") },
                    { "City", new DynamicValue("Metropolis") }
                })
            }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.NotNull(person?.HomeAddress);
        Assert.Equal("123 Main St", person.HomeAddress.Street);
        Assert.Equal("Metropolis", person.HomeAddress.City);
    }
    
    [Fact]
    public void MapTo_Enumerable_ShouldThrow()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "Enumerable", new DynamicValue("test") }
        });
    
        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => DynamicValueMapper.MapTo<Person>(dynamicValue));
    }
    
    [Fact]
    public void MapTo_List_ShouldMapCorrectly()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            {
                "List", new DynamicValue(new DynamicValueList(new List<DynamicValue>
                {
                    new("test1"),
                    new("test2")
                }))
            }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.NotNull(person?.List);
        Assert.Equal("test1", person.List[0]);
        Assert.Equal("test2", person.List[1]);
    }
    
    [Fact]
    public void MapTo_List_ShouldSkipWhenDifferentTypesAreUsed()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            {
                "List", new DynamicValue(new DynamicValueList(new List<DynamicValue>
                {
                    new("test1"),
                    new(1)
                }))
            }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.NotNull(person?.List);
        Assert.Equal("test1", person.List[0]);
        Assert.Null(person.List[1]);
    }
    
    [Fact]
    public void MapTo_Int_ShouldThrowWhenValueIsTooBig()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "Enumerable", new DynamicValue(long.MaxValue) }
        });
    
        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => DynamicValueMapper.MapTo<Person>(dynamicValue));
    }
    
    [Fact]
    public void MapTo_Long_ShouldMapCorrectly()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "LongValue", new DynamicValue(50) }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.Equal(50, person?.LongValue);
    }
    
    [Fact]
    public void MapTo_Bool_ShouldMapCorrectly()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "BoolValue", new DynamicValue(true) }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.True(person?.BoolValue);
    }
    
    [Fact]
    public void MapTo_Decimal_ShouldMapCorrectly()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "DecimalValue", new DynamicValue(1.0) }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.Equal((decimal)1.0, person?.DecimalValue);
    }
    
    [Fact]
    public void MapTo_Double_ShouldMapCorrectly()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "DoubleValue", new DynamicValue(1.0) }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.Equal(1.0, person?.DoubleValue);
    }
    
    [Fact]
    public void MapTo_DateTime_ShouldMapCorrectly()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "DateTimeValue", new DynamicValue(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)) }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), person?.DateTimeValue);
    }
    
    
    [Fact]
    public void MapTo_DateTimeOffset_ShouldMapCorrectly()
    {
        // Arrange
        var dynamicValue = new DynamicValue(new Dictionary<string, DynamicValue?>
        {
            { "DateTimeOffsetValue", new DynamicValue(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)) }
        });
    
        // Act
        var person = DynamicValueMapper.MapTo<Person>(dynamicValue);
    
        // Assert
        Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), person?.DateTimeOffsetValue);
    }
    
    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; } = 0;
        public Address? HomeAddress { get; set; }
        
        public IEnumerable<string>? Enumerable { get; set; }
        
        public List<string>? List { get; set; }
        
        public bool BoolValue { get; set; }
        
        public long? LongValue { get; set; }
        
        public decimal? DecimalValue { get; set; }
        
        public double? DoubleValue { get; set; }
        
        public DateTime? DateTimeValue { get; set; }

        public DateTimeOffset? DateTimeOffsetValue { get; set; }
    }

    private class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
