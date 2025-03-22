using System.Collections.Immutable;
using NSubstitute;

public class SaverTests
{
    private readonly IRepositoryElementStorage _repositoryElementStorage = Substitute.For<IRepositoryElementStorage>();
    private readonly IVariableStorage _variableStorage = Substitute.For<IVariableStorage>();
    private readonly IJsonSerializer _serializer = Substitute.For<IJsonSerializer>();
    private readonly IRepository _repository = Substitute.For<IRepository>();
    private readonly IOutputService _outputService = Substitute.For<IOutputService>();
    private readonly Saver _saver;

    public SaverTests()
    {
        _saver = new Saver(
            _repositoryElementStorage,
            _variableStorage,
            _serializer,
            _repository,
            _outputService
        );
    }

    [Fact]
    public async Task Execute_ShouldCallSaveRepositoryForEachModifiedElement()
    {
        // Arrange
        var sessionVariables = new List<Variable>
        {
            new Variable { Key = "var1", RepositoryElementId = "session1" },
            new Variable { Key = "var2", RepositoryElementId = "session1" }
        };

        var applicationVariables = new List<Variable>
        {
            new Variable { Key = "var3", RepositoryElementId = "app1" }
        };

        _variableStorage.Session.Returns(sessionVariables);
        _variableStorage.Application.Returns(applicationVariables);

        _repositoryElementStorage.Get().Returns(ImmutableList<RepositoryElement>.Empty);

        _serializer.TrySerialize(Arg.Any<RepositoryElementContent>(), out Arg.Any<string?>(), out Arg.Any<Exception?>())
            .Returns(call =>
            {
                call[1] = "{}"; // Set the serialized content
                call[2] = null; // No exception
                return true;    // Serialization succeeds
            });

        // Act
        await _saver.Execute(CancellationToken.None);

        // Assert
        await _repository.Received(1).SaveRepositoryElement("session1", "{}", Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveRepositoryElement("app1", "{}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ShouldLogDebugMessages()
    {
        // Arrange
        _variableStorage.Session.Returns(new List<Variable>()); // Return an empty List<Variable>
        _variableStorage.Application.Returns(new List<Variable>()); // Return an empty List<Variable>

        // Act
        await _saver.Execute(CancellationToken.None);

        // Assert
        _outputService.Received().Trace(Arg.Is<string>(msg => msg.Contains("Starting the save process")));
        _outputService.Received().Trace(Arg.Is<string>(msg => msg.Contains("Save process completed")));
    }

    [Fact]
    public async Task SaveRepository_ShouldSkipWhenContentIsNull()
    {
        // Arrange
        var sessionVariables = new List<Variable>
        {
            new Variable { Key = "var1", RepositoryElementId = "session1" }
        };

        var applicationVariables = new List<Variable>(); // Ensure this is an empty list, not null.

        _variableStorage.Session.Returns(sessionVariables);
        _variableStorage.Application.Returns(applicationVariables);

        // Mock repository elements to simulate a valid repository without content.
        var repositoryElements = ImmutableList.Create(
            new RepositoryElement(RepositoryLocation.Session, "session1", null) // OriginalRepository exists but has null Content.
        );
        _repositoryElementStorage.Get().Returns(repositoryElements);

        // Act
        await _saver.Execute(CancellationToken.None);

        // Assert
        _outputService.Received().Warning(Arg.Is<string>(msg => msg.Contains("Skipping saving to session1 as its content was not loaded.")));
        await _repository.DidNotReceiveWithAnyArgs().SaveRepositoryElement(default!, default!,default!);
    }


    [Fact]
    public async Task SaveRepository_ShouldHandleSerializationFailure()
    {
        // Arrange
        var sessionVariables = new List<Variable>
        {
            new Variable { Key = "var1", RepositoryElementId = "session1" }
        };

        var applicationVariables = new List<Variable>(); // Ensure this is an empty list, not null.

        _variableStorage.Session.Returns(sessionVariables);
        _variableStorage.Application.Returns(applicationVariables); // Mock application variables.

        var repositoryElements = ImmutableList<RepositoryElement>.Empty; // Mocked repository elements.
        _repositoryElementStorage.Get().Returns(repositoryElements);

        Exception serializationException = new Exception("Serialization failed");
        _serializer.TrySerialize(Arg.Any<RepositoryElementContent>(), out Arg.Any<string?>(), out Arg.Any<Exception?>())
            .Returns(call =>
            {
                call[2] = serializationException; // Set the out exception.
                return false; // Serialization fails.
            });

        // Act
        await Assert.ThrowsAsync<CommandWeaverException>(async () => await _saver.Execute(CancellationToken.None));
    }

    [Fact]
    public async Task SaveRepository_ShouldSkipSavingWhenOriginalContentIsMissing()
    {
        // Arrange
        var sessionVariables = new List<Variable>
        {
            new Variable { Key = "var1", RepositoryElementId = "session1" }
        };

        var applicationVariables = new List<Variable>(); // Ensure this is an empty list, not null.

        _variableStorage.Session.Returns(sessionVariables);
        _variableStorage.Application.Returns(applicationVariables);

        var repositoryElements = ImmutableList.Create(
            new RepositoryElement(RepositoryLocation.Session, "session1", null) // OriginalRepository exists but has null Content.
        );
        _repositoryElementStorage.Get().Returns(repositoryElements);

        // Act
        await _saver.Execute(CancellationToken.None);

        // Assert
        _outputService.Received().Warning(Arg.Is<string>(msg => msg.Contains("Skipping saving to session1 as its content was not loaded.")));
        await _repository.DidNotReceiveWithAnyArgs().SaveRepositoryElement(default!, default!, default);
    }

    [Fact]
    public async Task Execute_ShouldHandleEmptyModificationsGracefully()
    {
        // Arrange
        _variableStorage.Session.Returns(new List<Variable>()); // Use List<Variable> instead of Enumerable.Empty<Variable>()
        _variableStorage.Application.Returns(new List<Variable>()); // Use List<Variable> instead of Enumerable.Empty<Variable>()

        // Act
        await _saver.Execute(CancellationToken.None);

        // Assert
        await _repository.DidNotReceiveWithAnyArgs().SaveRepositoryElement(default!, default!, default);
    }

}
