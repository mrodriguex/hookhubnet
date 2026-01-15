using System.Net.Sockets;
using System.Net.WebSockets;

namespace HookHubNet.Common.DTOs;

/// <summary>
/// Represents information about a hook connection, including the WebSocket, port, and associated listener.
/// </summary>
public class HookInfo
{
    /// <summary>
    /// The WebSocket connection for this hook.
    /// </summary>
    public WebSocket WebSocket { get; set; } = null!;

    /// <summary>
    /// The port number associated with this hook.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// The TCP listener for handling incoming connections, if applicable.
    /// </summary>
    public TcpListener? Listener { get; set; }

    /// <summary>
    /// The cancellation token source for stopping the listener task.
    /// </summary>
    public CancellationTokenSource? CancellationTokenSource { get; set; }
}