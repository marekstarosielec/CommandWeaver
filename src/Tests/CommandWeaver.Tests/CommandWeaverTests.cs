using NSubstitute;

public class CommandWeaverTests
{
    private readonly ICommandService _commandService;
    private readonly ILoader _loader;
    private readonly ISaver _saver;
    private readonly IOutputService _outputService;
    private readonly IOutputSettings _outputSettings;
    private readonly ICommandParameterResolver _commandParameterResolver;
    private readonly ICommandValidator _commandValidator;
    private readonly IRepositoryElementStorage _repositoryElementStorage;
    private readonly IBackgroundService _backgroundService;
    private readonly CommandWeaver _commandWeaver;
    public CommandWeaverTests()
    {
        _commandService = Substitute.For<ICommandService>();
        _loader = Substitute.For<ILoader>();
        _saver = Substitute.For<ISaver>();
        _outputService = Substitute.For<IOutputService>();
        _outputSettings = Substitute.For<IOutputSettings>();
        _commandParameterResolver = Substitute.For<ICommandParameterResolver>();
        _commandValidator = Substitute.For<ICommandValidator>();
        _repositoryElementStorage = Substitute.For<IRepositoryElementStorage>();
        _backgroundService = Substitute.For<IBackgroundService>();

        _commandWeaver = new CommandWeaver(
            _commandService,
            _loader,
            _saver,
            _outputService,
            _outputSettings,
            _commandParameterResolver,
            _commandValidator,
            _repositoryElementStorage,
            _backgroundService
        );
    }

    [Fact]
    public async Task Run_ShouldTerminateWhenLogLevelIsIncorrect()
    {
        // Act
        await Assert.ThrowsAsync<CommandWeaverException>(async () => await _commandWeaver.Run(null!, new Dictionary<string, string>() {{"log-level", "abc"}}, CancellationToken.None));

        // Assert
        await _loader.DidNotReceive().Execute(Arg.Any<CancellationToken>());
        _commandValidator.DidNotReceive().ValidateCommands(Arg.Any<IEnumerable<RepositoryElement>>());
    }
    
    [Fact]
    public async Task Run_ShouldSetLogLevelWhenItIsCorrect()
    {
        // Act
        await Assert.ThrowsAsync<CommandWeaverException>(async () => await _commandWeaver.Run(null!, new Dictionary<string, string>() {{"log-level", "debug"}}, CancellationToken.None));

        // Assert
        _outputSettings.Received().CurrentLogLevel = LogLevel.Debug;
    }
    
    [Fact]
    public async Task Run_ShouldTerminateWhenCommandNameIsNull()
    {
        // Act
        await Assert.ThrowsAsync<CommandWeaverException>(async () => await _commandWeaver.Run(null!, new Dictionary<string, string>(), CancellationToken.None));

        // Assert
        await _loader.DidNotReceive().Execute(Arg.Any<CancellationToken>());
        _commandValidator.DidNotReceive().ValidateCommands(Arg.Any<IEnumerable<RepositoryElement>>());
    }

    [Fact]
    public async Task Run_ShouldTerminateWhenCommandIsUnknown()
    {
        // Arrange
        _commandService.Get("unknown-command").Returns((Command)null!);

        // Act
        await Assert.ThrowsAsync<CommandWeaverException>(async () => await _commandWeaver.Run("unknown-command", new Dictionary<string, string>(), CancellationToken.None));

        // Assert
        _commandParameterResolver.DidNotReceive().PrepareCommandParameters(Arg.Any<Command>(), Arg.Any<Dictionary<string, string>>());
        await _saver.DidNotReceive().Execute(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_ShouldExecuteSuccessfully()
    {
        // Arrange
        var command = new Command
        {
            Name = new DynamicValue("test-command"),
            Operations = new List<DynamicValue>()
        };
        _commandService.Get("test-command").Returns(command);

        // Act
        await _commandWeaver.Run("test-command", new Dictionary<string, string>(), CancellationToken.None);

        // Assert
        await _loader.Received(1).Execute(Arg.Any<CancellationToken>());
        _commandValidator.Received(1).ValidateCommands(Arg.Any<IEnumerable<RepositoryElement>>());
        _commandParameterResolver.Received(1).PrepareCommandParameters(command, Arg.Any<Dictionary<string, string>>());
        await _commandService.Received(1).ExecuteOperations(command.Operations, Arg.Any<CancellationToken>());
        await _saver.Received(1).Execute(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_ShouldLogStartAndCompletionMessages()
    {
        // Arrange
        var command = new Command
        {
            Name = new DynamicValue("test-command"),
            Operations = new List<DynamicValue>()
        };
        _commandService.Get("test-command").Returns(command);

        // Act
        await _commandWeaver.Run("test-command", new Dictionary<string, string>(), CancellationToken.None);

        // Assert
        _outputService.Received(1).Trace("Starting execution for command: test-command");
        _outputService.Received(1).Trace("Execution completed for command: test-command");
    }
}
