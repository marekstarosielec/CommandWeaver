using System.Reflection;
using System.Runtime.CompilerServices;

/// <inheritdoc />
public class EmbeddedRepository : IEmbeddedRepository
{
    /// <inheritdoc />
    public async IAsyncEnumerable<RepositoryElementInfo> GetList([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var baseName = assembly.GetName().Name ?? string.Empty;
        var resourceNames = assembly.GetManifestResourceNames();

        foreach (var resourceName in resourceNames)
        {
            await using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                continue;

            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            var format = GetFileFormat(resourceName);
            var friendlyName = GetFriendlyName(resourceName, baseName, format);

            yield return new RepositoryElementInfo
            {
                Id = resourceName,
                Format = format,
                FriendlyName = friendlyName,
                Content = content
            };
        }
    }

    /// <summary>
    /// Extract aprt after last dot - it should be extension.
    /// </summary>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    private static string GetFileFormat(string resourceName) =>
        resourceName.LastIndexOf('.') is var pos && pos >= 0 ? resourceName[(pos + 1)..] : string.Empty;

    /// <summary>
    /// Removes assembly name and extension to get user friendly name.
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="baseName"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    private static string GetFriendlyName(string resourceName, string baseName, string format)
    {
        return resourceName.Length > baseName.Length + format.Length + 1
            ? resourceName[(baseName.Length + 1)..^(format.Length + 1)].Replace('.', '/')
            : resourceName;
    }
}