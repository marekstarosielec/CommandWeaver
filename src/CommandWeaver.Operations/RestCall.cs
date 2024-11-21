using System.Text;

public record RestCall(IConditionsService conditionsService, IVariables variables, ISerializerFactory serializerFactory) : Operation
{
    public override string Name => nameof(RestCall);

    public override Dictionary<string, OperationParameter> Parameters { get; } = new Dictionary<string, OperationParameter>
    {
        {"url", new OperationParameter { Description = "The endpoint of the API to call", RequiredText = true} },
        {"method", new OperationParameter { Description = "Operation to perform", RequiredText = true, AllowedValues = [HttpMethod.Get.ToString(), HttpMethod.Post.ToString(), HttpMethod.Put.ToString(), HttpMethod.Delete.ToString(), HttpMethod.Patch.ToString() ] } },
        {"headers", new OperationParameter { Description = "Metadata for the request"} },
        {"body", new OperationParameter { Description = "Data sent with the request"} },
        {"timeout", new OperationParameter { Description = "Seconds to wait for a response before failing"} },
        {"certificate", new OperationParameter { Description = "Path to certificate file"} },
        {"certificatePassword", new OperationParameter { Description = "Certificate password if any"} }
    };
    
    public override async Task Run(CancellationToken cancellationToken)
    {
        var httpClient = new HttpClient();
        AddHeaders(httpClient);
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Parse(Parameters["method"].Value.TextValue!);
        request.RequestUri = new Uri(Parameters["url"].Value.TextValue!);
        httpClient.Timeout = TimeSpan.FromSeconds(Parameters["timeout"].Value.NumericValue ?? 60);
        var serializer = serializerFactory.GetDefaultSerializer(out _);
        if (Parameters["body"].Value.TextValue != null)
            request.Content = new StringContent(Parameters["body"].Value.TextValue!, Encoding.UTF8, GetContentType() ?? "application/json");
        if (Parameters["body"].Value.ObjectValue != null || Parameters["body"].Value.ListValue != null)
        {
            serializer.TrySerialize(Parameters["body"].Value, out var body, out _);
            request.Content = new StringContent(body, Encoding.UTF8,"application/json");
        }

        var result = await httpClient.SendAsync(request, cancellationToken);
        var resultBody = await result.Content.ReadAsStringAsync(cancellationToken);
        //try to deserialize to json
        serializer.TryDeserialize(resultBody, out DynamicValue resultModel, out _);
        
        // 
        // lastRestCall["response"] = resultBody;
        var dynamicValueResponse =  new Dictionary<string, DynamicValue?>();
        dynamicValueResponse["status"] = new DynamicValue((int)result.StatusCode);
        dynamicValueResponse["body"] = resultModel;
        
        var lastRestCall = new Dictionary<string, DynamicValue?>();
        lastRestCall["response"] = new DynamicValue(dynamicValueResponse);
        variables.WriteVariableValue(VariableScope.Command, "lastRestCall", new DynamicValue(lastRestCall));
        var t = variables.ReadVariableValue(new DynamicValue("lastRestCall"), true);
    }

    private string? GetContentType() => Parameters["headers"].Value.ListValue?.FirstOrDefault(h => string.Equals(h["key"]?.TextValue, "content-type", StringComparison.InvariantCultureIgnoreCase))?["value"].TextValue;

    private void AddHeaders(HttpClient httpClient)
    {
        // var headers = Parameters["headers"].Value.ListValue;
        // if (headers == null)
        //     return;
        //
        // foreach (var header in headers)
        // {
        //     var skipHeader = false;
        //     string? name = null;
        //     string? value = null;
        //     foreach (var headerKey in header.Keys)
        //         if (string.Equals(headerKey, "conditions", StringComparison.OrdinalIgnoreCase))
        //         {
        //             var condition = conditionsService.GetFromDynamicValue(header[headerKey]);
        //             if (condition != null && conditionsService.ShouldBeSkipped(condition, variables))
        //                 skipHeader = true;
        //         }
        //         else
        //         {
        //             name = headerKey;
        //             value = header[headerKey].TextValue;
        //         }
        //     
        //     if (!skipHeader && name!=null)
        //         if (!httpClient.DefaultRequestHeaders.TryAddWithoutValidation(name, value))
        //         {
        //             
        //         }
        // }
    }
}