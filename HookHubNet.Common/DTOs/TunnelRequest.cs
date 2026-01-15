namespace HookHubNet.Common.DTOs;

/// <summary>
/// Represents a request to create a tunnel to a specific target host and port.
/// </summary>
public class TunnelRequest
{
    /// <summary>
    /// The unique identifier for this tunnel.
    /// </summary>
    public string TunnelId { get; set; } = "";

    /// <summary>
    /// The target host to connect to (e.g., 127.0.0.1).
    /// </summary>
    public string TargetHost { get; set; } = "";

    /// <summary>
    /// The target port to connect to (e.g., 5000).
    /// </summary>
    public int TargetPort { get; set; }
}