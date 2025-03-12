using System.Net;

/// <summary>
/// Service for handling tasks that run in background and need to be completed before cloaing app.
/// </summary>
public interface IBackgroundService
{
    /// <summary>
    /// Create and run new HttpListener.
    /// </summary>
    /// <param name="port"></param>
    /// <param name="requestHandler"></param>
    /// <param name="token"></param>
    void CreateHttpListener(int port, Func<HttpListenerContext, CancellationToken, Task> requestHandler, CancellationToken token);

    /// <summary>
    /// Waits until all background services completed;
    /// </summary>
    /// <returns></returns>
    Task WaitToComplete();
}