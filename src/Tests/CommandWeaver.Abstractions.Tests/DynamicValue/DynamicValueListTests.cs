public class DynamicValueListTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmptyList_WhenNoParametersProvided()
    {
        // Arrange & Act
        var list = new DynamicValueList();

        // Assert
        Assert.Empty(list);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithGivenItems()
    {
        // Arrange
        var items = new List<DynamicValueObject>
        {
            new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key1", new DynamicValue("value1") } }),
            new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key2", new DynamicValue("value2") } })
        };

        // Act
        var list = new DynamicValueList(items);

        // Assert
        Assert.Equal(2, list.Count());
        Assert.Equal(items[0], list[0]);
        Assert.Equal(items[1], list[1]);
    }

    [Fact]
    public void Indexer_ShouldReturnItemAtIndex()
    {
        // Arrange
        var item = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value") } });
        var list = new DynamicValueList(new List<DynamicValueObject> { item });

        // Act & Assert
        Assert.Equal(item, list[0]);
    }

    [Fact]
    public void Add_ShouldReturnNewListWithItemAdded()
    {
        // Arrange
        var initialList = new DynamicValueList();
        var newItem = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value") } });

        // Act
        var updatedList = initialList.Add(newItem);

        // Assert
        Assert.Empty(initialList); // Ensure immutability
        Assert.Single(updatedList);
        Assert.Equal(newItem, updatedList[0]);
    }

    [Fact]
    public void FirstOrDefault_ShouldReturnFirstItem_WhenListIsNotEmpty()
    {
        // Arrange
        var item = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value") } });
        var list = new DynamicValueList(new List<DynamicValueObject> { item });

        // Act
        var result = list.FirstOrDefault();

        // Assert
        Assert.Equal(item, result);
    }

    [Fact]
    public void FirstOrDefault_ShouldReturnNull_WhenListIsEmpty()
    {
        // Arrange
        var list = new DynamicValueList();

        // Act
        var result = list.FirstOrDefault();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FirstOrDefault_WithPredicate_ShouldReturnMatchingItem()
    {
        // Arrange
        var item1 = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value1") } });
        var item2 = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value2") } });
        var list = new DynamicValueList(new List<DynamicValueObject> { item1, item2 });

        // Act
        var result = list.FirstOrDefault(obj => obj["key"]?.TextValue == "value2");

        // Assert
        Assert.Equal(item2, result);
    }

    [Fact]
    public void FirstOrDefault_WithPredicate_ShouldReturnNull_WhenNoMatchFound()
    {
        // Arrange
        var item = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value") } });
        var list = new DynamicValueList(new List<DynamicValueObject> { item });

        // Act
        var result = list.FirstOrDefault(obj => obj["key"]?.TextValue == "abc");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RemoveAll_ShouldReturnNewListWithoutMatchingItems()
    {
        // Arrange
        var item1 = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value1") } });
        var item2 = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key", new DynamicValue("value2") } });
        var list = new DynamicValueList(new List<DynamicValueObject> { item1, item2 });

        // Act
        var updatedList = list.RemoveAll(obj => obj["key"].TextValue == item1["key"].TextValue);

        // Assert
        Assert.Single(updatedList);
        Assert.Equal(item2, updatedList[0]);
    }

    [Fact]
    public void GetEnumerator_ShouldEnumerateItems()
    {
        // Arrange
        var item1 = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key1", new DynamicValue("value1") } });
        var item2 = new DynamicValueObject(new Dictionary<string, DynamicValue?> { { "key2", new DynamicValue("value2") } });
        var list = new DynamicValueList(new List<DynamicValueObject> { item1, item2 });

        // Act & Assert
        Assert.Collection(list,
            i => Assert.Equal(item1, i),
            i => Assert.Equal(item2, i));
    }
}
