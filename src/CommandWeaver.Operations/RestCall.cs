using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public record RestCall(IConditionsService conditionsService, IVariableService variableServices, IJsonSerializer serializer, IFlowService flowService, IOutputService outputService) : Operation
{
    public override string Name => nameof(RestCall);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init;  } = new Dictionary<string, OperationParameter>
    {
        {"url", new OperationParameter { Description = "The endpoint of the API to call", Validation = new Validation { Required = true, AllowedType = "text"}} },
        {"method", new OperationParameter { Description = "Operation to perform",  Validation = new Validation{ Required = true, AllowedTextValues = [HttpMethod.Get.ToString(), HttpMethod.Post.ToString(), HttpMethod.Put.ToString(), HttpMethod.Delete.ToString(), HttpMethod.Patch.ToString() ] } }},
        {"headers", new OperationParameter { Description = "Metadata for the request"} },
        {"body", new OperationParameter { Description = "Data sent with the request"} },
        {"timeout", new OperationParameter { Description = "Seconds to wait for a response before failing"} },
        {"certificate", new OperationParameter { Description = "Path to certificate file"} },
        {"certificatePassword", new OperationParameter { Description = "Certificate password if any"} }
    }.ToImmutableDictionary();
    
    public override async Task Run(CancellationToken cancellationToken)
    {
        var certificateInformation = Parameters["certificate"].Value.GetAsObject<Certificate>();
        
        using var handler = new HttpClientHandler();
        if (!string.IsNullOrWhiteSpace(certificateInformation?.FromResource))
        {
            var resourceKey = $"{{{{resources[{certificateInformation.FromResource}].binary}}}}";
            var certificateBinaryContent = variableServices.ReadVariableValue(new DynamicValue(resourceKey));
            if (certificateBinaryContent.LazyBinaryValue?.Value != null)
            {
                try
                {
                    var certificate =
                        new X509Certificate2(certificateBinaryContent.LazyBinaryValue.Value,
                            Parameters["certificatePassword"].Value.TextValue);
                    handler.ClientCertificates.Add(certificate);
                }
                catch (Exception e)
                {
                    flowService.FatalException(e, "Failed to load certificate from certificate resource");
                    throw;
                }
            }
        }

        using var httpClient = new HttpClient(handler);
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Parse(Parameters["method"].Value.TextValue!);
        request.RequestUri = new Uri(Parameters["url"].Value.TextValue!);
        httpClient.Timeout = TimeSpan.FromSeconds(Parameters["timeout"].Value.NumericValue ?? 60);
        AddBody(request, out var jsonBody);
        AddHeaders(request);
        outputService.WriteRequest(request, jsonBody, Parameters["body"].Value.TextValue);
        
        
        var result = await httpClient.SendAsync(request, cancellationToken);
        var resultBody = await result.Content.ReadAsStringAsync(cancellationToken);
        
        //try to deserialize to json
        serializer.TryDeserialize(resultBody, out DynamicValue? resultModel, out _);
        
        // 
        // lastRestCall["response"] = resultBody;
        var dynamicValueResponse =  new Dictionary<string, DynamicValue?>();
        dynamicValueResponse["status"] = new DynamicValue((int)result.StatusCode);
        dynamicValueResponse["body"] = resultModel;
        
        var lastRestCall = new Dictionary<string, DynamicValue?>();
        lastRestCall["response"] = new DynamicValue(dynamicValueResponse);
        variableServices.WriteVariableValue(VariableScope.Command, "lastRestCall", new DynamicValue(lastRestCall));
        
        var t = variableServices.ReadVariableValue(new DynamicValue("lastRestCall"), true);
    }

    private void AddBody(HttpRequestMessage request, out string? jsonBody)
    {
        jsonBody = null;
        if (Parameters["body"].Value.TextValue != null)
            request.Content = new StringContent(Parameters["body"].Value.TextValue!, Encoding.UTF8, GetContentType() ?? "application/json");
        if (Parameters["body"].Value.ObjectValue != null || Parameters["body"].Value.ListValue != null)
        {
            serializer.TrySerialize(Parameters["body"].Value, out jsonBody, out _);
            if (!string.IsNullOrEmpty(jsonBody))
                request.Content = new StringContent(jsonBody, Encoding.UTF8,"application/json");
        }
    }

    private string? GetContentType() => Parameters["headers"].Value.ListValue?.FirstOrDefault(h => string.Equals(h.ObjectValue?["key"]?.TextValue, "content-type", StringComparison.OrdinalIgnoreCase))?.ObjectValue?["value"].TextValue;

    private void AddHeaders(HttpRequestMessage request)
    {
        var headers = Parameters["headers"].Value.ListValue;
        if (headers == null)
            return;

        foreach (var header in headers)
        {
            var name = header.ObjectValue?["name"]?.TextValue;
            if (string.IsNullOrEmpty(name))
            {
                flowService.Terminate("Missing header name");
                return;
            }

            var value = header.ObjectValue?["value"]?.TextValue;
            if (string.IsNullOrEmpty(value))
            {
                flowService.Terminate("Missing header value");
                return;
            }

            var conditions = header.ObjectValue?["conditions"];
            Condition? parsedConditions = null;
            if (conditions != null && !conditions.IsNull())
                parsedConditions = conditionsService.GetFromDynamicValue(conditions);
            if (parsedConditions != null && conditionsService.ConditionsAreMet(parsedConditions))
                continue;

            if (!request.Headers.TryAddWithoutValidation(name, value))
            {
                if (request.Content == null)
                {
                    flowService.Terminate($"Failed to add header {name}");
                    return;
                }

                if (!request.Content.Headers.TryAddWithoutValidation(name!, value!))
                {
                    flowService.Terminate($"Failed to add header {name}");
                    return;
                }
            }
        }
    }

    private record Certificate
    {
        public string? FromResource { get; set; }
    }
}