public record RestCall : Operation
{
    public override string Name => nameof(RestCall);

    public override Dictionary<string, OperationParameter> Parameters { get; } = new Dictionary<string, OperationParameter>
    {
        {"url", new OperationParameter { Description = "The endpoint of the API to call", RequiredText = true} },
        {"method", new OperationParameter { Description = "Operation to perform", RequiredText = true, AllowedValues = [HttpMethod.Get.ToString(), HttpMethod.Post.ToString(), HttpMethod.Put.ToString(), HttpMethod.Delete.ToString(), HttpMethod.Patch.ToString() ] } },
        {"headers", new OperationParameter { Description = "Metadata for the request"} },
        {"body", new OperationParameter { Description = "Data sent with the request"} },
        {"timeout", new OperationParameter { Description = "How long to wait for a response before failing"} }
    };
    
    public override async Task Run(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Parse(Parameters["method"].Value.TextValue!);
        request.RequestUri = new Uri(Parameters["url"].Value.TextValue!);
        var httpClient = new HttpClient();
        var result = await httpClient.SendAsync(request, cancellationToken);
        var t = await result.Content.ReadAsStringAsync();
    }
}