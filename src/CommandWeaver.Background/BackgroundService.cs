using System.Net;

/// <inheritdoc />
public class BackgroundService(IFlowService flowService) : IBackgroundService
{
    private readonly Dictionary<int, HttpListenerInformation> _httpListeners = new();
    
    /// <inheritdoc />
    public void CreateHttpListener(int port, Func<HttpListenerContext, CancellationToken, Task> requestHandler, CancellationToken cancellationToken)
    {
        if (_httpListeners.ContainsKey(port))
        {
            flowService.Terminate($"There is already a listener on port {port}");
            throw new Exception($"There is already a listener on port {port}");
        }
        
        var httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://localhost:{port}/");
        httpListener.Start();

        var listenerTask = Task.Run(async () =>
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested) // Keep running until stopped
                {
                    var context = await httpListener.GetContextAsync();
                    await requestHandler(context, cancellationToken);
                }
            }
            catch (HttpListenerException ex)
            {
                flowService.Terminate($"Unexpected error in http listener on port {port}: {ex.Message}");
            }
            catch (ObjectDisposedException)
            {
                //Listener was closed externally
            }
            catch (Exception ex)
            {
                flowService.Terminate($"Unexpected error in http listener on port {port}: {ex.Message}");
            }
        }, cancellationToken);
        _httpListeners.Add(port, new HttpListenerInformation(httpListener, listenerTask));
    }

    /// <inheritdoc />
    public async Task WaitToComplete()
    {
        foreach (var httpListener in _httpListeners)
            await httpListener.Value.Task;
    }

    /// <inheritdoc />
    public void Stop()
    {
        foreach (var httpListener in _httpListeners)
            httpListener.Value.Listener.Stop();
        
        _httpListeners.Clear();
    }
}