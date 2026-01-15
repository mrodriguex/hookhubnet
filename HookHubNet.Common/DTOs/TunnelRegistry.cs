using System.Net.Sockets;
using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace HookHubNet.Common.DTOs;

/// <summary>
/// Registry for managing hooks and their associated tunnels.
/// </summary>
public class TunnelRegistry
{
    /// <summary>
    /// Dictionary mapping hook IDs to their HookInfo objects.
    /// </summary>
    public ConcurrentDictionary<string, HookInfo> Hooks { get; } = new();

    /// <summary>
    /// Dictionary mapping tunnel IDs to their NetworkStream objects.
    /// </summary>
    public ConcurrentDictionary<Guid, NetworkStream> Tunnels { get; } = new();
}
