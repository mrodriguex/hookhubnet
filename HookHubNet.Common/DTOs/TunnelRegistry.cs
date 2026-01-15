using System.Net.Sockets;
using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace HookHubNet.Common.DTOs;

public class TunnelRegistry
{
    public ConcurrentDictionary<string, HookInfo> Hooks { get; } = new();
    public ConcurrentDictionary<Guid, NetworkStream> Tunnels { get; } = new();
}
