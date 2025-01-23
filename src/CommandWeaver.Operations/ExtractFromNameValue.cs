using System.Collections.Immutable;
using System.Web;

public record ExtractFromNameValue(IVariableService variableService) : Operation
{
    public override string Name => nameof(ExtractFromNameValue);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } = new Dictionary<string, OperationParameter>
    {
        { "input", new OperationParameter { Description = "Url or name/value collection", Validation = new Validation { Required = true, AllowedType = "text"}}},
        { "name", new OperationParameter { Description = "Name to find", Validation = new Validation { Required = true, AllowedType = "text"}}}
    }.ToImmutableDictionary();
  
    public override Task Run(CancellationToken cancellationToken)
    {
        var input = Parameters["input"].Value.TextValue;
        var name = Parameters["name"].Value.TextValue;
        var result = ExtractValue(input!, name!);
        variableService.WriteVariableValue(VariableScope.Command, "name_value", new DynamicValue(result));
        return Task.CompletedTask;
    }
    
    private static string? ExtractValue(string input, string name)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(name))
            return null;

        // Check if the input contains a full URL
        var uri = Uri.TryCreate(input, UriKind.Absolute, out var tempUri) ? tempUri : null;
        var query = uri?.Query ?? input; // Use query from URL or treat input as query string

        // Combine query and fragment if available
        if (uri?.Fragment.StartsWith("#") == true)
            query += "&" + uri.Fragment.Substring(1);

        // Parse query string and fragment
        var collection = HttpUtility.ParseQueryString(query);

        // Return the value for the given name
        return collection[name];
    }
}