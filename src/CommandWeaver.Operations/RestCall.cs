public record RestCall(IConditionsService conditionsService, IVariables variables) : Operation
{
    public override string Name => nameof(RestCall);

    public override Dictionary<string, OperationParameter> Parameters { get; } = new Dictionary<string, OperationParameter>
    {
        {"url", new OperationParameter { Description = "The endpoint of the API to call", RequiredText = true} },
        {"method", new OperationParameter { Description = "Operation to perform", RequiredText = true, AllowedValues = [HttpMethod.Get.ToString(), HttpMethod.Post.ToString(), HttpMethod.Put.ToString(), HttpMethod.Delete.ToString(), HttpMethod.Patch.ToString() ] } },
        {"headers", new OperationParameter { Description = "Metadata for the request"} },
        {"body", new OperationParameter { Description = "Data sent with the request"} },
        {"timeout", new OperationParameter { Description = "How long to wait for a response before failing"} },
        {"certificate", new OperationParameter { Description = "Path to certificate file"} },
        {"certificatePassword", new OperationParameter { Description = "Certificate password if any"} }
    };
    
    public override async Task Run(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Parse(Parameters["method"].Value.TextValue!);
        request.RequestUri = new Uri(Parameters["url"].Value.TextValue!);
        var headers = Parameters["headers"].Value.ListValue;
        if (headers != null)
            foreach (var header in headers)
            {
                var skipHeader = false;
                string? name = null;
                string? value = null;
                foreach (var headerKey in header.Keys)
                {
                    if (string.Equals(headerKey, "conditions", StringComparison.OrdinalIgnoreCase))
                    {
                        var condition = conditionsService.GetFromDynamicValue(header[headerKey]);
                        if (condition != null && conditionsService.ShouldBeSkipped(condition, variables))
                            skipHeader = true;
                    }
                    else
                    {
                        name = headerKey;
                        value = header[headerKey].TextValue;
                    }
                }
                if (!skipHeader && name!=null)
                    request.Headers.Add(name, value);
            }
        
        var httpClient = new HttpClient();
        var result = await httpClient.SendAsync(request, cancellationToken);
        var t = await result.Content.ReadAsStringAsync(cancellationToken);
    }
}