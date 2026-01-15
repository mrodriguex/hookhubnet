using System.Net.Sockets;
using System.Net.WebSockets;

namespace HookHubNet.Common.DTOs;

public class HookInfo
{
    public WebSocket WebSocket { get; set; } = null!;
    public int Port { get; set; }
    public TcpListener? Listener { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; } // For stopping the listener task
}