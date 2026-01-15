using System.Net.Sockets;
using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Text;
using HookHubNet.Common;
using Microsoft.Extensions.Logging;

namespace HookHubNet.Hook.Services;

/// <summary>
/// Client that connects to the HookHub hub and forwards traffic to a local backend service.
/// It establishes a WebSocket connection to the hub and manages TCP tunnels to the target host.
/// </summary>
public class HookHubClient
{
    private readonly ILogger<HookHubClient> _logger;
    private readonly string _hubUrl;
    private readonly string _hookId;
    private readonly string _targetHost;
    private readonly int _targetPort;

    /// <summary>
    /// Initializes a new instance of the HookHubClient.
    /// </summary>
    /// <param name="logger">Logger for this client.</param>
    /// <param name="hubUrl">The base WebSocket URL of the hub to connect to (without id).</param>
    /// <param name="hookId">The ID of this hook.</param>
     /// <param name="targetHost">The hostname of the backend service to forward traffic to.</param>
    /// <param name="targetPort">The port of the backend service.</param>
    public HookHubClient(ILogger<HookHubClient> logger, string hubUrl, string hookId, string targetHost, int targetPort)
    {
        _logger = logger;
        _hubUrl = hubUrl;
        _hookId = hookId;
        _targetHost = targetHost;
        _targetPort = targetPort;
    }

    /// <summary>
    /// Runs the client, connecting to the hub and handling tunnel operations.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        // Establish WebSocket connection to the hub
        var ws = new ClientWebSocket();
        await ws.ConnectAsync(
            new Uri($"{_hubUrl}?id={_hookId}"),
            cancellationToken
        );

        _logger.LogInformation("Connected to hub at {HubUrl}", _hubUrl);
        _logger.LogInformation("Hook ID: {HookId}", _hookId);
        _logger.LogInformation("Forwarding to {TargetHost}:{TargetPort}", _targetHost, _targetPort);

        // Dictionary to track active tunnels (tunnel ID to TCP client)
        var tunnels = new ConcurrentDictionary<Guid, TcpClient>();

        try
        {
            // Main loop to receive and handle frames from the hub
            while (ws.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var (type, tunnelId, payload) =
                    await TunnelProtocol.ReceiveFrame(ws);

                // ======================
                // OPEN TUNNEL
                // ======================
                if (type == TunnelProtocol.OPEN)
                {
                    await HandleTunnelOpen(ws, tunnelId, payload, tunnels, cancellationToken);
                }

                // ======================
                // DATA FROM HUB → BACKEND
                // ======================
                else if (type == TunnelProtocol.DATA)
                {
                    await HandleTunnelData(tunnelId, payload, tunnels);
                }

                // ======================
                // CLOSE TUNNEL
                // ======================
                else if (type == TunnelProtocol.CLOSE)
                {
                    HandleTunnelClose(tunnelId, tunnels);
                }
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogError(ex, "WebSocket error");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Operation cancelled");
        }
        finally
        {
            // Clean up all tunnels
            foreach (var tunnel in tunnels.Values)
            {
                tunnel.Close();
            }
            tunnels.Clear();

            // Close the WebSocket if still open
            if (ws.State == WebSocketState.Open)
            {
                await ws.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client shutting down",
                    CancellationToken.None
                );
            }
        }
    }

    /// <summary>
    /// Handles opening a new tunnel by connecting to the backend and starting data forwarding.
    /// </summary>
    /// <param name="ws">The WebSocket connection to the hub.</param>
    /// <param name="tunnelId">The unique ID of the tunnel.</param>
    /// <param name="payload">The payload containing the target information.</param>
    /// <param name="tunnels">The dictionary of active tunnels.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleTunnelOpen(
        ClientWebSocket ws,
        Guid tunnelId,
        byte[] payload,
        ConcurrentDictionary<Guid, TcpClient> tunnels,
        CancellationToken cancellationToken)
    {
        // Decode the target from the payload (though it's not used in this implementation)
        var target = Encoding.UTF8.GetString(payload);
        _logger.LogInformation("Opening tunnel {TunnelId} for target: {Target}", tunnelId, target);

        // Connect to the backend service
        var tcp = new TcpClient();
        await tcp.ConnectAsync(_targetHost, _targetPort, cancellationToken);

        // Register the tunnel
        tunnels[tunnelId] = tcp;

        // Start a background task to read from backend and send to hub
        _ = Task.Run(async () =>
        {
            var stream = tcp.GetStream();
            var buffer = new byte[8192];
            int read;

            try
            {
                // Read data from backend and forward to hub
                while ((read = await stream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await TunnelProtocol.SendFrame(
                        ws,
                        TunnelProtocol.DATA,
                        tunnelId,
                        buffer[..read]
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from backend for tunnel {TunnelId}", tunnelId);
            }
            finally
            {
                // Send close frame to hub and clean up
                await TunnelProtocol.SendFrame(
                    ws,
                    TunnelProtocol.CLOSE,
                    tunnelId,
                    Array.Empty<byte>()
                );

                tunnels.TryRemove(tunnelId, out _);
                tcp.Close();
                _logger.LogDebug("Tunnel {TunnelId} closed", tunnelId);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Handles data received from the hub by writing it to the backend.
    /// </summary>
    /// <param name="tunnelId">The ID of the tunnel.</param>
    /// <param name="payload">The data payload to write.</param>
    /// <param name="tunnels">The dictionary of active tunnels.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleTunnelData(
        Guid tunnelId,
        byte[] payload,
        ConcurrentDictionary<Guid, TcpClient> tunnels)
    {
        // Find the tunnel and write the data to the backend
        if (tunnels.TryGetValue(tunnelId, out var tcp))
        {
            try
            {
                await tcp.GetStream().WriteAsync(payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to backend for tunnel {TunnelId}", tunnelId);
            }
        }
    }

    /// <summary>
    /// Handles closing a tunnel by removing it from the registry and closing the connection.
    /// </summary>
    /// <param name="tunnelId">The ID of the tunnel to close.</param>
    /// <param name="tunnels">The dictionary of active tunnels.</param>
    private void HandleTunnelClose(
        Guid tunnelId,
        ConcurrentDictionary<Guid, TcpClient> tunnels)
    {
        // Remove and close the tunnel
        if (tunnels.TryRemove(tunnelId, out var tcp))
        {
            tcp.Close();
            _logger.LogDebug("Tunnel {TunnelId} closed by hub", tunnelId);
        }
    }
}
