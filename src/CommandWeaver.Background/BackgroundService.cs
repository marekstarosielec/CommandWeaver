using System.Net;

/// <inheritdoc />
public class BackgroundService(IFlowService flowService) : IBackgroundService
{
    private readonly Dictionary<int, HttpListenerInformation> _httpListeners = new();
    
    /// <inheritdoc />
    public void CreateHttpListener(int port, Func<HttpListenerContext, CancellationToken, Task> requestHandler, CancellationToken cancellationToken)
    {
        if (_httpListeners.Count > 0)
        {
            flowService.Terminate($"There is already active listener on port {_httpListeners.FirstOrDefault().Key}. Only one active listener is allowed.");
            //TODO: This is workaround to avoid concurrency problem when accessing variables.
            // Ideas how to solve this problem:
            // 1. Allow to define variable name containing incoming request, but this will not allow to use operations in variables (they will not know which variable contains request).
            // 2. Allow to define context identifier for execution (e.g. port number). Variable service would access variables from given context, or from no context, but never from another context. Seems complicated and might not solve problem.
            // 3. Instead of running operations in own thread, add it to queue of operations in main thread, so they are executed linearly. This might be tricky, but can solve lots of problems. Problem: how to send response to sender.
            return;
        }
        if (_httpListeners.ContainsKey(port))
        {
            flowService.Terminate($"There is already a listener on port {port}");
            return;
        }
        
        var httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://127.0.0.1:{port}/");
        try
        {
            httpListener.Start();
        }
        catch (HttpListenerException e) when (e.Message == "Address already in use")
        {
            flowService.Terminate($"There is already a listener on port {port}");
        }
        
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