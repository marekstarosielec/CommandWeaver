using System.Collections.Immutable;
using NSubstitute;

public class LoaderTests
{
    private readonly IVariableService _variableService;
    private readonly IEmbeddedRepository _embeddedRepository;
    private readonly IRepository _repository;
    private readonly IOutputService _outputService;
    private readonly IJsonSerializer _serializer;

    private readonly Loader _loader;

    public LoaderTests()
    {
        _variableService = Substitute.For<IVariableService>();
        _embeddedRepository = Substitute.For<IEmbeddedRepository>();
        _repository = Substitute.For<IRepository>();
        _outputService = Substitute.For<IOutputService>();
        var outputSettings = Substitute.For<IOutputSettings>();
        var commandService = Substitute.For<ICommandService>();
        _serializer = Substitute.For<IJsonSerializer>();
        var repositoryElementStorage = Substitute.For<IRepositoryElementStorage>();
        var resourceService = Substitute.For<IResourceService>();

        _loader = new Loader(
            _variableService,
            _embeddedRepository,
            _repository,
            _outputService,
            outputSettings,
            commandService,
            _serializer,
            repositoryElementStorage,
            resourceService);
    }

    [Fact]
    public async Task Execute_ShouldLogExecutionStartAndCompletion()
    {
        // Arrange
        _embeddedRepository.GetList(Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<RepositoryElementInformation>());

        // Act
        await _loader.Execute(CancellationToken.None);

        // Assert
        _outputService.Received().Trace(Arg.Is<string>(msg => msg.Contains("Execution started")));
        _outputService.Received().Trace(Arg.Is<string>(msg => msg.Contains("Execution completed")));
    }

    [Fact]
    public async Task LoadBuiltInRepository_ShouldLogDebugMessage()
    {
        // Arrange
        _embeddedRepository.GetList(Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<RepositoryElementInformation>());

        // Act
        await _loader.Execute(CancellationToken.None);

        // Assert
        _outputService.Received().Trace(Arg.Is<string>(msg => msg.Contains("built-in repository")));
    }

    [Fact]
    public async Task LoadRepositoryElements_ShouldLogWarningsForEmptyContent()
    {
        // Arrange
        var repositoryElement = new RepositoryElementInformation
        {
            Id = "Id",
            FriendlyName = "EmptyElement",
            Format = "json",
            ContentAsString = new Lazy<string?>(() => string.Empty)
        };

        _embeddedRepository.GetList(Arg.Any<CancellationToken>())
            .Returns(ToAsyncEnumerable(repositoryElement));

        // Act
        await _loader.Execute(CancellationToken.None);

        // Assert
        _outputService.Received().Warning(Arg.Is<string>(msg => msg.Contains("empty") && msg.Contains("EmptyElement")));
    }

    [Fact]
    public async Task LoadRepositoryElements_ShouldLogDeserializationWarnings()
    {
        // Arrange
        var repositoryElement = new RepositoryElementInformation
        {
            Id = "Id",
            FriendlyName = "InvalidElement",
            Format = "json",
            ContentAsString = new Lazy<string?>(() => "{ invalid json }")
        };

        _embeddedRepository.GetList(Arg.Any<CancellationToken>())
            .Returns(ToAsyncEnumerable(repositoryElement));
        _serializer.TryDeserialize(Arg.Any<string>(), out Arg.Any<RepositoryElementContent>()!, out Arg.Any<Exception>()!)
            .Returns(x =>
            {
                x[1] = null; // repositoryContent
                x[2] = new Exception("Deserialization error");
                return false;
            });

        // Act
        await _loader.Execute(CancellationToken.None);

        // Assert
        _outputService.Received().Warning(Arg.Is<string>(msg => msg.Contains("Failed to deserialize") && msg.Contains("InvalidElement")));
    }

    [Fact]
    public async Task LoadRepositoryElements_ShouldLogSkippedElementsForUnsupportedFormat()
    {
        // Arrange
        var repositoryElement = new RepositoryElementInformation
        {
            Id = "Id",
            FriendlyName = "UnsupportedElement",
            Format = "xml",
            ContentAsString = new Lazy<string?>(() => "<unsupported></unsupported>")
        };

        _embeddedRepository.GetList(Arg.Any<CancellationToken>())
            .Returns(ToAsyncEnumerable(repositoryElement));

        // Act
        await _loader.Execute(CancellationToken.None);

        // Assert
        _outputService.Received().Trace(Arg.Is<string>(msg => msg.Contains("Skipped") && msg.Contains("UnsupportedElement")));
    }

    [Fact]
    public async Task LoadRepositoryElements_ShouldLogAddedCommandsAndVariables()
    {
        // Arrange
        var repositoryContent = new RepositoryElementContent
        {
            Commands = ImmutableList.Create(new Command { Name = new DynamicValue("TestCommand") })!,
            Variables = ImmutableList.Create(new Variable { Key = "TestVariable" })!
        };

        var repositoryElement = new RepositoryElementInformation
        {
            Id = "Id",
            FriendlyName = "ValidElement",
            Format = "json",
            ContentAsString = new Lazy<string?>(() => "{ valid json }")
        };

        _embeddedRepository.GetList(Arg.Any<CancellationToken>())
            .Returns(ToAsyncEnumerable(repositoryElement));
        _serializer.TryDeserialize(Arg.Any<string>(), out Arg.Any<RepositoryElementContent>()!, out Arg.Any<Exception>()!)
            .Returns(x =>
            {
                x[1] = repositoryContent;
                x[2] = null;
                return true;
            });

        // Act
        await _loader.Execute(CancellationToken.None);

        // Assert
        _outputService.Received().Trace(Arg.Is<string>(msg => msg.Contains("Added variables")));
        _outputService.Received().Trace(Arg.Is<string>(msg => msg.Contains("Added commands")));
    }

    [Fact]
    public async Task Execute_ShouldSetCommonVariablesAndLog()
    {
        // Arrange
        _repository.GetPath(Arg.Is(RepositoryLocation.Application)).Returns("/application/path");
        _repository.GetPath(Arg.Is(RepositoryLocation.Session), Arg.Any<string>()).Returns("/session/path");

        // Act
        await _loader.Execute(CancellationToken.None);

        // Assert
        _outputService.Received().Trace(Arg.Is<string>(msg => msg.Contains("Setting common variables")));
        _variableService.Received().WriteVariableValue(VariableScope.Command, "LocalPath", Arg.Is<DynamicValue>(v => v.TextValue == "/application/path"));
        _variableService.Received().WriteVariableValue(VariableScope.Command, "SessionPath", Arg.Is<DynamicValue>(v => v.TextValue == "/session/path"));
    }
    
    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(T value)
    {
        yield return value;
        await Task.CompletedTask;
    }
}
