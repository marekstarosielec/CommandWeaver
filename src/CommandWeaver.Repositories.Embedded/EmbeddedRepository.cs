using System.Reflection;
using System.Runtime.CompilerServices;

/// <inheritdoc />
public class EmbeddedRepository : IEmbeddedRepository
{
    private readonly Assembly _assembly;
    private readonly string _resourcePrefix;
    private readonly IOutputService _outputService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedRepository"/> class using the executing assembly.
    /// </summary>
    /// <param name="outputService">The service used for logging output.</param>
    public EmbeddedRepository(IOutputService outputService)
        : this(Assembly.GetExecutingAssembly(), string.Empty, outputService) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedRepository"/> class with a specified assembly and resource prefix.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resources.</param>
    /// <param name="resourcePrefix">The prefix to filter relevant embedded resources.</param>
    /// <param name="outputService">The service used for logging output.</param>
    internal EmbeddedRepository(Assembly assembly, string resourcePrefix, IOutputService outputService)
    {
        _assembly = assembly;
        _resourcePrefix = resourcePrefix;
        _outputService = outputService;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<RepositoryElementSerialized> GetList([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var baseName = _assembly.GetName().Name ?? string.Empty;
        var resourceNames = FilterResourceNames(_assembly.GetManifestResourceNames());

        foreach (var resourceName in resourceNames)
        {
            var element = await CreateRepositoryElement(resourceName, baseName, cancellationToken);
            if (element != null)
                yield return element;
            else
                _outputService.Trace($"Resource '{resourceName}' could not be loaded.");
        }
    }

    /// <summary>
    /// Filters resource names to include only JSON files with the specified prefix.
    /// </summary>
    /// <param name="resourceNames">The list of resource names to filter.</param>
    /// <returns>A filtered list of resource names that are JSON files.</returns>
    private IEnumerable<string> FilterResourceNames(IEnumerable<string> resourceNames) =>
        resourceNames.Where(name => name.StartsWith(_resourcePrefix, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Creates a <see cref="RepositoryElementSerialized"/> from a resource name.
    /// </summary>
    /// <param name="resourceName">The resource name.</param>
    /// <param name="baseName">The base name of the assembly.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="RepositoryElementSerialized"/> or <c>null</c> if the resource cannot be processed.</returns>
    private async Task<RepositoryElementSerialized?> CreateRepositoryElement(string resourceName, string baseName, CancellationToken cancellationToken)
    {
        await using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            _outputService.Debug($"Stream for resource '{resourceName}' is null.");
            return null;
        }

        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        return new RepositoryElementSerialized
        {
            Id = resourceName,
            Format = "json",
            FriendlyName = GetFriendlyName(resourceName, baseName),
            Content = content
        };
    }

    /// <summary>
    /// Extracts a user-friendly name from the resource name.
    /// </summary>
    /// <param name="resourceName">The resource name.</param>
    /// <param name="baseName">The base name of the assembly.</param>
    /// <returns>A user-friendly name for the resource.</returns>
    private static string GetFriendlyName(string resourceName, string baseName) =>
        resourceName.Length > baseName.Length
            ? resourceName[(baseName.Length + 1)..]
            : resourceName;
}
