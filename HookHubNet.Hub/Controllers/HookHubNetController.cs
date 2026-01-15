using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using HookHubNet.Common;
using System.Net;
using System.Net.Sockets;
using HookHubNet.Common.DTOs;

namespace HookHubNet.Hub.Controllers;

/// <summary>
/// Controller for handling WebSocket connections from hooks and managing TCP tunnels.
/// </summary>
[ApiController]
[Route("hookhubnet")]
public class HookHubNetController : ControllerBase
{
    private readonly TunnelRegistry _registry;

    /// <summary>
    /// Initializes a new instance of the HookHubNetController.
    /// </summary>
    /// <param name="registry">The tunnel registry for managing hooks and tunnels.</param>
    public HookHubNetController(TunnelRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Handles WebSocket connections from hooks, assigns ports, and manages tunnel operations.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpGet]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        var hookId = HttpContext.Request.Query["id"].FirstOrDefault() ?? "default";

        // Check if a hook with the same ID is already connected
        if (_registry.Hooks.ContainsKey(hookId))
        {
            HttpContext.Response.StatusCode = 409; // Conflict
            Console.WriteLine($"Hook '{hookId}' already connected. Rejecting new connection.");
            return;
        }

        var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();

        // Assign unique port and create listener
        //var port = Interlocked.Increment(ref _nextPort) - 1; // Thread-safe increment
        var listener = new TcpListener(IPAddress.Any, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var cts = new CancellationTokenSource();

        var hookInfo = new HookInfo { WebSocket = ws, Port = port, Listener = listener, CancellationTokenSource = cts };
        _registry.Hooks[hookId] = hookInfo;

        Console.WriteLine($"Hook '{hookId}' connected on port {port}");

        // Start handling TCP clients for this hook in a background task
        _ = Task.Run(() => HandleTcpClientsForHook(hookId, hookInfo, cts.Token));

        try
        {
            while (ws.State == WebSocketState.Open)
            {
                var (type, tunnelId, payload) = await TunnelProtocol.ReceiveFrame(ws);

                if (type == TunnelProtocol.DATA && _registry.Tunnels.TryGetValue(tunnelId, out var stream))
                {
                    await stream.WriteAsync(payload);
                }
                else if (type == TunnelProtocol.CLOSE && _registry.Tunnels.TryRemove(tunnelId, out var s))
                {
                    s.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hook '{hookId}' error: {ex.Message}");
        }
        finally
        {
            // Cleanup on disconnect
            cts.Cancel();
            listener.Stop();
            _registry.Hooks.TryRemove(hookId, out _);
            Console.WriteLine($"Hook '{hookId}' disconnected, port {port} stopped");
        }
    }

    /// <summary>
    /// Handles incoming TCP clients for a specific hook by accepting connections and creating tunnels.
    /// </summary>
    /// <param name="hookId">The ID of the hook.</param>
    /// <param name="hookInfo">The hook information including the listener.</param>
    /// <param name="token">Cancellation token for stopping the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleTcpClientsForHook(string hookId, HookInfo hookInfo, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var client = await hookInfo.Listener!.AcceptTcpClientAsync(token);
                _ = HandleClient(client, hookInfo.WebSocket);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on cancellation
        }
    }

    /// <summary>
    /// Handles an individual TCP client connection by creating a tunnel and forwarding data.
    /// </summary>
    /// <param name="client">The connected TCP client.</param>
    /// <param name="hook">The WebSocket connection to the hook.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleClient(TcpClient client, WebSocket hook)
    {
        var clientStream = client.GetStream();
        var tunnelId = Guid.NewGuid();

        _registry.Tunnels[tunnelId] = clientStream;

        await TunnelProtocol.SendFrame(hook, TunnelProtocol.OPEN, tunnelId, Array.Empty<byte>());

        var buffer = new byte[8192];
        int read;

        try
        {
            while ((read = await clientStream.ReadAsync(buffer)) > 0)
            {
                await TunnelProtocol.SendFrame(hook, TunnelProtocol.DATA, tunnelId, buffer[..read]);
            }
        }
        finally
        {
            await TunnelProtocol.SendFrame(hook, TunnelProtocol.CLOSE, tunnelId, Array.Empty<byte>());
            _registry.Tunnels.TryRemove(tunnelId, out _);
            client.Close();
        }
    }
}
