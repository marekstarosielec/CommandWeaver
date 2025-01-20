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
        using var handler = GetHttpClientHandler();
        using var httpClient = new HttpClient(handler);
        
        using var request = GetHttpRequestMessage(httpClient);
        await outputService.WriteRequest(request);
        
        var response = await httpClient.SendAsync(request, cancellationToken);
        var resultBody = await response.Content.ReadAsStringAsync(cancellationToken);
        await outputService.WriteResponse(response);
        
        var lastRestCall = new Dictionary<string, DynamicValue?>();
        lastRestCall["request"] = await GetRequestAsVariable(request);
        lastRestCall["response"] = await GetResponseAsVariable(response); 
        variableServices.WriteVariableValue(VariableScope.Command, "lastRestCall", new DynamicValue(lastRestCall));
        
    }

    private async Task<DynamicValue> GetRequestAsVariable(HttpRequestMessage request)
    {
        var result = new Dictionary<string, DynamicValue?>
        {
            ["method"] = new (request.Method.ToString()),
            ["url"] = new (request.RequestUri?.AbsoluteUri)
        };
        var body = request.Content != null ? await request.Content.ReadAsStringAsync() : null;
        
        if (!string.IsNullOrWhiteSpace(body) && JsonHelper.IsJson(body) && serializer.TryDeserialize(body, out DynamicValue? bodyModel, out _))
            result["body"] = bodyModel;
        else
            result["body"] = new DynamicValue(body);

        var headers = new List<DynamicValue>();
        foreach (var header in request.Headers.Concat(request.Content?.Headers.ToList() ?? []))
        {
            var headerVariable = new Dictionary<string, DynamicValue?>();
            headerVariable["key"] = new DynamicValue(header.Key);
            headerVariable["value"] = new DynamicValue(string.Join(',', header.Value));
            headers.Add(new DynamicValue(headerVariable));
        }

        result["headers"] = new DynamicValue(headers);
        
        return new DynamicValue(result);
    }
    
    private async Task<DynamicValue> GetResponseAsVariable(HttpResponseMessage response)
    {
        var result = new Dictionary<string, DynamicValue?>
        {
            ["status"] = new ((int)response.StatusCode)
        };
        var body = await response.Content.ReadAsStringAsync();
        
        if (!string.IsNullOrWhiteSpace(body) && JsonHelper.IsJson(body) && serializer.TryDeserialize(body, out DynamicValue? bodyModel, out _))
            result["body"] = bodyModel;
        else
            result["body"] = new DynamicValue(body);

        var headers = new List<DynamicValue>();
        foreach (var header in response.Headers.Concat(response.Content.Headers))
        {
            var headerVariable = new Dictionary<string, DynamicValue?>();
            headerVariable["key"] = new DynamicValue(header.Key);
            headerVariable["value"] = new DynamicValue(string.Join(',', header.Value));
            headers.Add(new DynamicValue(headerVariable));
        }

        result["headers"] = new DynamicValue(headers);
        
        return new DynamicValue(result);
    }
    
    private HttpRequestMessage GetHttpRequestMessage(HttpClient httpClient)
    {
        HttpRequestMessage? request = null;
        try
        {
            request = new HttpRequestMessage();
            request.Method = HttpMethod.Parse(Parameters["method"].Value.TextValue!);
            request.RequestUri = new Uri(Parameters["url"].Value.TextValue!);
            httpClient.Timeout = TimeSpan.FromSeconds(Parameters["timeout"].Value.NumericValue ?? 60);
            GetBody(request);
            GetHeaders(request);
            return request;
        }
        catch
        {
            request?.Dispose();
            throw;
        }
    }

    private HttpClientHandler GetHttpClientHandler()
    {
        var certificateInformation = Parameters["certificate"].Value.GetAsObject<Certificate>();

        HttpClientHandler? handler = null;
        try
        {
            handler = new HttpClientHandler();
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

            return handler;
        }
        catch
        {
            handler?.Dispose();
            throw;
        }
    }

    private void GetBody(HttpRequestMessage request)
    {
        if (Parameters["body"].Value.ObjectValue != null || Parameters["body"].Value.ListValue != null)
        {
            serializer.TrySerialize(Parameters["body"].Value, out var jsonBody, out _);
            if (!string.IsNullOrEmpty(jsonBody))
                request.Content = new StringContent(jsonBody, Encoding.UTF8,"application/json");
        }
        if (Parameters["body"].Value.TextValue != null && request.Content == null)
            request.Content = new StringContent(Parameters["body"].Value.TextValue!, Encoding.UTF8, GetContentType() ?? "application/json");

    }

    private string? GetContentType() => Parameters["headers"].Value.ListValue?.FirstOrDefault(h => string.Equals(h.ObjectValue?["key"]?.TextValue, "content-type", StringComparison.OrdinalIgnoreCase))?.ObjectValue?["value"].TextValue;

    private void GetHeaders(HttpRequestMessage request)
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