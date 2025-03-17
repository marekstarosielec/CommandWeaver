using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
// ReSharper disable ClassNeverInstantiated.Global

public record RestServer(IBackgroundService backgroundService, IOutputService outputService, ICommandService commandService, IVariableService variableService, IJsonSerializer serializer) : Operation
{
    public override string Name => nameof(RestServer);

    public override ImmutableDictionary<string, OperationParameter> Parameters { get; init; } =
        new Dictionary<string, OperationParameter>
        {
            { 
                "port",
                new OperationParameter
                {
                    Description = "Port where calls are received",
                    Validation = new Validation { Required = true, AllowedType = "number" }
                }
            },
            { 
                "endpoints",
                new OperationParameter
                {
                    Description = "Definition of endpoints and operations executed when request is received",
                    Validation = new Validation { Required = true, AllowedStrongType = typeof(List<EndpointDefinition>)}
                }
            }}.ToImmutableDictionary();
    
    public override Task Run(CancellationToken cancellationToken)
    {
        var port = Parameters["port"].Value.GetAsObject<int>();
        var endpoints = Parameters["endpoints"].Value.GetAsObject<List<EndpointDefinition>>();
        if (endpoints?.Any() != true)
        {
            outputService.Debug("No endpoint definitions found");
            return Task.CompletedTask;
        }
        
        backgroundService.CreateHttpListener(port, RequestReceived, cancellationToken);
        return Task.CompletedTask;
    }

    private async Task RequestReceived(HttpListenerContext context, CancellationToken cancellationToken)
    {
        var endpoints = Parameters["endpoints"].Value.GetAsObject<List<EndpointDefinition>>() ?? [];

        var request = context.Request;
        var response = context.Response;
        
        var requestVariable = await GetRequestAsVariable(request, cancellationToken);
        variableService.WriteVariableValue(VariableScope.Command, "rest_request", requestVariable);

        foreach (var endpoint in endpoints)
            foreach (var url in endpoint.Url ?? ["^.*$"])
                if (Regex.IsMatch(request.Url?.AbsoluteUri ?? string.Empty, url))
                {
                    if (endpoint.Events?.RequestReceived != null)
                        await commandService.ExecuteOperations(endpoint.Events.RequestReceived, cancellationToken);
                    
                    response.StatusCode = endpoint.ResponseCode;
                    if (endpoint.ResponseBody != null)
                    {
                        var buffer = Encoding.UTF8.GetBytes(endpoint.ResponseBody);
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                    }
                    response.OutputStream.Close();
                    return;
                }
     
        // var defaultResponseBuffer = Encoding.UTF8.GetBytes("Request received.");
        // await response.OutputStream.WriteAsync(defaultResponseBuffer, 0, defaultResponseBuffer.Length, cancellationToken);
        response.OutputStream.Close();
    }
    
    private async Task<DynamicValue> GetRequestAsVariable(HttpListenerRequest request, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, DynamicValue?>
        {
            ["method"] = new (request.HttpMethod.ToString()),
            ["url"] = new (request.Url?.AbsoluteUri),
            ["created"] = new (DateTime.UtcNow.ToString("O"))
        };
        var body = "";
        if (request.HasEntityBody)
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            body = await reader.ReadToEndAsync(cancellationToken);
        }
        result["body"] = new DynamicValue(body);
        if (!string.IsNullOrWhiteSpace(body) && JsonHelper.IsJson(body) && serializer.TryDeserialize(body, out DynamicValue? bodyModel, out _))
            result["body_json"] = bodyModel;

        var headers = new List<DynamicValue>();
        foreach (var header in request.Headers.AllKeys)
        {
            var headerVariable = new Dictionary<string, DynamicValue?>();
            headerVariable["key"] = new DynamicValue(header);
            headerVariable["value"] = new DynamicValue(string.Join(',', request.Headers[header]));
            headers.Add(new DynamicValue(headerVariable));
        }

        result["headers"] = new DynamicValue(headers);
        
        return new DynamicValue(result);
    }
    
    public class RestServerEvents
    {
        public List<DynamicValue>? RequestReceived { get; set; }
    }
    
    public class EndpointDefinition
    {
        [Required]
        public List<string>? Url { get; set; }
    
        public string? ResponseBody { get; set; }

        public int ResponseCode { get; set; } = 200;
        
        public RestServerEvents? Events { get; set; }
    }
}

