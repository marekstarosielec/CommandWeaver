public class DynamicValueListTests
{
    [Fact]
    public void Constructor_WithEmptyList_ShouldInitializeCorrectly()
    {
        // Act
        var dynamicValueList = new DynamicValueList();

        // Assert
        Assert.Equal(0, dynamicValueList.Count);
    }

    [Fact]
    public void Constructor_WithValidList_ShouldInitializeCorrectly()
    {
        // Arrange
        var items = new List<DynamicValue>
        {
            new ("value1"),
            new (42)
        };

        // Act
        var dynamicValueList = new DynamicValueList(items);

        // Assert
        Assert.Equal(2, dynamicValueList.Count);
        Assert.Equal("value1", dynamicValueList[0].TextValue);
        Assert.Equal(42, dynamicValueList[1].NumericValue);
    }

    [Fact]
    public void Add_ShouldReturnNewInstanceWithItemAdded()
    {
        // Arrange
        var dynamicValueList = new DynamicValueList();
        var newValue = new DynamicValue("newItem");

        // Act
        var updatedList = dynamicValueList.Add(newValue);

        // Assert
        Assert.Equal(0, dynamicValueList.Count); // Original list should remain unchanged
        Assert.Equal(1, updatedList.Count);
        Assert.Equal("newItem", updatedList[0].TextValue);
    }

    [Fact]
    public void AddRange_ShouldReturnNewInstanceWithItemsAdded()
    {
        // Arrange
        var dynamicValueList = new DynamicValueList();
        var newItems = new List<DynamicValue>
        {
            new ("item1"),
            new (100)
        };

        // Act
        var updatedList = dynamicValueList.AddRange(newItems);

        // Assert
        Assert.Equal(0, dynamicValueList.Count); // Original list should remain unchanged
        Assert.Equal(2, updatedList.Count);
        Assert.Equal("item1", updatedList[0].TextValue);
        Assert.Equal(100, updatedList[1].NumericValue);
    }

    [Fact]
    public void AddRange_WithNullItems_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dynamicValueList = new DynamicValueList();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => dynamicValueList.AddRange(null!));
        Assert.Contains("Items cannot be null", exception.Message);
    }

    [Fact]
    public void Indexer_ShouldReturnCorrectItem()
    {
        // Arrange
        var items = new List<DynamicValue>
        {
            new ("value1"),
            new (42)
        };
        var dynamicValueList = new DynamicValueList(items);

        // Act
        var item = dynamicValueList[1];

        // Assert
        Assert.Equal(42, item.NumericValue);
    }

    [Fact]
    public void FirstOrDefault_WithMatchingPredicate_ShouldReturnCorrectItem()
    {
        // Arrange
        var items = new List<DynamicValue>
        {
            new DynamicValue("value1"),
            new DynamicValue("value2"),
            new DynamicValue(42)
        };
        var dynamicValueList = new DynamicValueList(items);

        // Act
        var result = dynamicValueList.FirstOrDefault(x => x.TextValue == "value2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("value2", result.TextValue);
    }

    [Fact]
    public void FirstOrDefault_WithNonMatchingPredicate_ShouldReturnNull()
    {
        // Arrange
        var items = new List<DynamicValue>
        {
            new DynamicValue("value1"),
            new DynamicValue("value2"),
            new DynamicValue(42)
        };
        var dynamicValueList = new DynamicValueList(items);

        // Act
        var result = dynamicValueList.FirstOrDefault(x => x.TextValue == "value3");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RemoveAll_ShouldReturnNewInstanceWithoutMatchingItems()
    {
        // Arrange
        var items = new List<DynamicValue>
        {
            new DynamicValue("value1"),
            new DynamicValue("value2"),
            new DynamicValue(42)
        };
        var dynamicValueList = new DynamicValueList(items);

        // Act
        var updatedList = dynamicValueList.RemoveAll(x => x.TextValue == "value2");

        // Assert
        Assert.Equal(3, dynamicValueList.Count); // Original list should remain unchanged
        Assert.Equal(2, updatedList.Count);
        Assert.Equal("value1", updatedList[0].TextValue);
        Assert.Equal(42, updatedList[1].NumericValue);
    }

    [Fact]
    public void Enumerator_ShouldIterateOverItems()
    {
        // Arrange
        var items = new List<DynamicValue>
        {
            new ("value1"),
            new (42)
        };
        var dynamicValueList = new DynamicValueList(items);

        // Act
        using var enumerator = dynamicValueList.GetEnumerator();
        var enumeratedItems = new List<DynamicValue>();
        while (enumerator.MoveNext())
        {
            enumeratedItems.Add(enumerator.Current);
        }

        // Assert
        Assert.Equal(2, enumeratedItems.Count);
        Assert.Equal("value1", enumeratedItems[0].TextValue);
        Assert.Equal(42, enumeratedItems[1].NumericValue);
    }
}
