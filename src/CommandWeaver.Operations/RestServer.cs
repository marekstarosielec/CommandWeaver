using System.Collections.Immutable;
using System.Net;
using System.Text;

public record RestServer(IBackgroundService backgroundService) : Operation
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
        }.ToImmutableDictionary();
    
    public override Task Run(CancellationToken cancellationToken)
    {
        var port = Parameters["port"].Value.NumericValue!.Value;
        backgroundService.CreateHttpListener((int)port, RequestReceived, cancellationToken);
        return Task.CompletedTask;
    }

    private static async Task RequestReceived(HttpListenerContext context, CancellationToken cancellationToken)
    {
        var request = context.Request;
        var response = context.Response;

        var body = "";
        if (request.HasEntityBody)
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            body = await reader.ReadToEndAsync(cancellationToken);
        }

        Console.WriteLine($"[{request.HttpMethod}] {request.Url}");
        Console.WriteLine($"Headers: {string.Join("; ", request.Headers.AllKeys)}");
        Console.WriteLine($"Body: {body}");

        var buffer = Encoding.UTF8.GetBytes("Request received.");
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        response.OutputStream.Close();
    }
}