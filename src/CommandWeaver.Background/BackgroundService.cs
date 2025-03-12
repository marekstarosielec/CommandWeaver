using System.Net;

/// <inheritdoc />
public class BackgroundService(IFlowService flowService) : IBackgroundService
{
    private readonly Dictionary<int, Task> _backgroundTasks = new();
    
    /// <inheritdoc />
    public void CreateHttpListener(int port, Func<HttpListenerContext, CancellationToken, Task> requestHandler, CancellationToken cancellationToken)
    {
        if (_backgroundTasks.ContainsKey(port))
        {
            flowService.Terminate($"There is already a listener on port {port}");
            throw new Exception($"There is already a listener on port {port}");
        }
        
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        var listenerTask = Task.Run(async () =>
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested) // Keep running until stopped
                {
                    var context = await listener.GetContextAsync();
                    await requestHandler(context, cancellationToken);
                }
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"Listener error: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Listener task canceled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex}");
            }
        }, cancellationToken);
        _backgroundTasks.Add(port, listenerTask);
    }

    /// <inheritdoc />
    public async Task WaitToComplete()
    {
        foreach (var backgroundTask in _backgroundTasks)
            await backgroundTask.Value;
    }
}