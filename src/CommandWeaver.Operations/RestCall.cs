using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public record RestCall(IConditionsService conditionsService, IVariableService variableService, IJsonSerializer serializer, IOutputService outputService, ICommandService commandService) : Operation
{
    public override string Name => nameof(RestCall);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } = new Dictionary<string, OperationParameter>
    {
        {"url", new OperationParameter { Description = "The endpoint of the API to call", Validation = new Validation { Required = true, AllowedType = "text"}} },
        {"method", new OperationParameter { Description = "Operation to perform",  Validation = new Validation{ Required = true, AllowedTextValues = [HttpMethod.Get.ToString(), HttpMethod.Post.ToString(), HttpMethod.Put.ToString(), HttpMethod.Delete.ToString(), HttpMethod.Patch.ToString() ] } }},
        {"headers", new OperationParameter { Description = "Metadata for the request"} },
        {"body", new OperationParameter { Description = "Data sent with the request"} },
        {"timeout", new OperationParameter { Description = "Seconds to wait for a response before failing"} },
        {"certificate", new OperationParameter { Description = "Path to certificate file"} },
        {"certificatePassword", new OperationParameter { Description = "Certificate password if any"} },
        {"events", new OperationParameter { Description = "Operations executed in response to given event", Validation = new Validation{ AllowedStrongType = typeof(RestCallEvents)}} }
    }.ToImmutableDictionary();
    
    public override async Task Run(CancellationToken cancellationToken)
    {
        using var handler = GetHttpClientHandler();
        using var httpClient = new HttpClient(handler);
        var events = Parameters["events"].OriginalValue.GetAsObject<RestCallEvents>();
       
        using var request = GetHttpRequestMessage(httpClient);
        var requestVariable = await GetRequestAsVariable(request);
        variableService.WriteVariableValue(VariableScope.Command, "rest_request", requestVariable);
        
        if (events?.RequestPrepared != null)
            await commandService.ExecuteOperations(events.RequestPrepared, cancellationToken);
        
        var response = await httpClient.SendAsync(request, cancellationToken);
        
        var responseVariable = await GetResponseAsVariable(response);
        variableService.WriteVariableValue(VariableScope.Command, "rest_response", responseVariable);
        
        if (events?.ResponseReceived != null)
            await commandService.ExecuteOperations(events.ResponseReceived, cancellationToken);
    }

    private async Task<DynamicValue> GetRequestAsVariable(HttpRequestMessage request)
    {
        var result = new Dictionary<string, DynamicValue?>
        {
            ["method"] = new (request.Method.ToString()),
            ["url"] = new (request.RequestUri?.AbsoluteUri),
            ["created"] = new (DateTime.UtcNow.ToString("O"))
        };
        var body = request.Content != null ? await request.Content.ReadAsStringAsync() : null;
        
        result["body"] = new DynamicValue(body);
        if (!string.IsNullOrWhiteSpace(body) && JsonHelper.IsJson(body) && serializer.TryDeserialize(body, out DynamicValue? bodyModel, out _))
            result["body_json"] = bodyModel;

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
            ["status"] = new ((int)response.StatusCode),
            ["created"] = new (DateTime.UtcNow.ToString("O")),
            ["success"] = new (response.IsSuccessStatusCode)
        };
        var body = await response.Content.ReadAsStringAsync();
        
        result["body"] = new DynamicValue(body);
        if (!string.IsNullOrWhiteSpace(body) && JsonHelper.IsJson(body) && serializer.TryDeserialize(body, out DynamicValue? bodyModel, out _))
            result["body_json"] = bodyModel;
        
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
                var certificateBinaryContent = variableService.ReadVariableValue(new DynamicValue(resourceKey));
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
                        throw new CommandWeaverException("Failed to load certificate from certificate resource", innerException: e);
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
                throw new CommandWeaverException("Missing header name");

            var value = header.ObjectValue?["value"]?.TextValue;
            if (string.IsNullOrEmpty(value))
                throw new CommandWeaverException("Missing header value");
            
            var conditions = header.ObjectValue?["conditions"];
            Condition? parsedConditions = null;
            if (conditions != null && !conditions.IsNull())
                parsedConditions = conditionsService.GetFromDynamicValue(conditions);
            if (parsedConditions != null && conditionsService.ConditionsAreMet(parsedConditions))
                continue;

            if (!request.Headers.TryAddWithoutValidation(name, value))
            {
                if (request.Content == null)
                    throw new CommandWeaverException($"Failed to add header {name}");

                if (!request.Content.Headers.TryAddWithoutValidation(name!, value!))
                    throw new CommandWeaverException($"Failed to add header {name}");
            }
        }
    }

    private record Certificate
    {
        public string? FromResource { get; set; }
    }

    private record RestCallEvents
    {
        public List<DynamicValue>? RequestPrepared { get; set; }
        public List<DynamicValue>? ResponseReceived { get; set; }
    }
}