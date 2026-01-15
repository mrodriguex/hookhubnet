namespace HookHubNet.Common.DTOs;

/// <summary>
/// Represents data for a tunnel operation, including the tunnel ID, payload data, and close flag.
/// </summary>
public class TunnelData
{
    /// <summary>
    /// The unique identifier of the tunnel.
    /// </summary>
    public string TunnelId { get; set; } = "";

    /// <summary>
    /// The data payload for the tunnel.
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Indicates whether the tunnel should be closed.
    /// </summary>
    public bool Close { get; set; } = false;
}