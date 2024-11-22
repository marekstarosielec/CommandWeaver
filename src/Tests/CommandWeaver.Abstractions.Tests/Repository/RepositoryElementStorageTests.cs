using System.Collections.Immutable;

public class RepositoryElementStorageTests
{
    [Fact]
    public void Add_ShouldAddRepositoryElementToStorage()
    {
        // Arrange
        var storage = new RepositoryElementStorage();
        var repositoryElement = new RepositoryElement(RepositoryLocation.Application, "1", null);

        // Act
        storage.Add(repositoryElement);

        // Assert
        var result = storage.Get();
        Assert.Single(result);
        Assert.Equal(repositoryElement, result[0]);
    }

    [Fact]
    public void Get_ShouldReturnImmutableList()
    {
        // Arrange
        var storage = new RepositoryElementStorage();
        var repositoryElement1 = new RepositoryElement(RepositoryLocation.Application, "1", null);
        var repositoryElement2 = new RepositoryElement(RepositoryLocation.Session, "2", null);
        
        storage.Add(repositoryElement1);
        storage.Add(repositoryElement2);

        // Act
        var result = storage.Get();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(repositoryElement1, result);
        Assert.Contains(repositoryElement2, result);

        // Check immutability
        Assert.IsType<ImmutableList<RepositoryElement>>(result);
    }

    [Fact]
    public void Get_ShouldReturnEmptyList_WhenNoElementsAreAdded()
    {
        // Arrange
        var storage = new RepositoryElementStorage();

        // Act
        var result = storage.Get();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Add_ShouldStoreMultipleElements()
    {
        // Arrange
        var storage = new RepositoryElementStorage();
        var repositoryElement1 = new RepositoryElement(RepositoryLocation.BuiltIn, "1", null);
        var repositoryElement2 = new RepositoryElement(RepositoryLocation.Session, "2", null);

        // Act
        storage.Add(repositoryElement1);
        storage.Add(repositoryElement2);

        // Assert
        var result = storage.Get();
        Assert.Equal(2, result.Count);
        Assert.Contains(repositoryElement1, result);
        Assert.Contains(repositoryElement2, result);
    }

    [Fact]
    public void Add_ShouldNotModifyExistingElements()
    {
        // Arrange
        var storage = new RepositoryElementStorage();
        var repositoryElement = new RepositoryElement(RepositoryLocation.BuiltIn, "1", null);
        storage.Add(repositoryElement);

        // Act
        var resultBefore = storage.Get();
        storage.Add(new RepositoryElement(RepositoryLocation.Application, "2", null));
        var resultAfter = storage.Get();

        // Assert
        Assert.Single(resultBefore); // Initial list should be unchanged
        Assert.Equal("1", resultBefore[0].Id);
        Assert.Equal(2, resultAfter.Count); // Total elements after addition
    }
}
