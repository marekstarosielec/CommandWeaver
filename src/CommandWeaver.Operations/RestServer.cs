using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
// ReSharper disable ClassNeverInstantiated.Global

public record RestServer(IBackgroundService backgroundService, IOutputService outputService, ICommandService commandService) : Operation
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
            }
        }.ToImmutableDictionary();
    
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
        
        var body = "";
        if (request.HasEntityBody)
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            body = await reader.ReadToEndAsync(cancellationToken);
        }

        foreach (var endpoint in endpoints)
            foreach (var url in endpoint.Url ?? [])
                if (Regex.IsMatch(request.Url?.AbsoluteUri ?? string.Empty, url))
                {
                    response.StatusCode = endpoint.ResponseCode;
                    if (endpoint.ResponseBody != null)
                    {
                        var buffer = Encoding.UTF8.GetBytes(endpoint.ResponseBody);
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                    }
                    response.OutputStream.Close();
                    return;
                }
     
        Console.WriteLine($"[{request.HttpMethod}] {request.Url}");
        Console.WriteLine($"Headers: {string.Join("; ", request.Headers.AllKeys)}");
        Console.WriteLine($"Body: {body}");

        var defaultResponseBuffer = Encoding.UTF8.GetBytes("Request received.");
        await response.OutputStream.WriteAsync(defaultResponseBuffer, 0, defaultResponseBuffer.Length, cancellationToken);
        response.OutputStream.Close();
    }
}

public class EndpointDefinition
{
    [Required]
    public List<string>? Url { get; set; }
    
    public string? ResponseBody { get; set; }

    public int ResponseCode { get; set; } = 200;
}