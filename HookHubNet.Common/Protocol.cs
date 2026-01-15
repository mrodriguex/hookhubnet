using System.Net.WebSockets;

namespace HookHubNet.Common;

/// <summary>
/// Provides static methods for handling the tunnel protocol over WebSocket connections.
/// The protocol uses binary frames with a type byte, a 16-byte GUID for tunnel ID, and a payload.
/// </summary>
public static class TunnelProtocol
{
    /// <summary>
    /// Frame type for opening a tunnel.
    /// </summary>
    public const byte OPEN  = 1;

    /// <summary>
    /// Frame type for sending data through a tunnel.
    /// </summary>
    public const byte DATA  = 2;

    /// <summary>
    /// Frame type for closing a tunnel.
    /// </summary>
    public const byte CLOSE = 3;

    /// <summary>
    /// Sends a frame over the WebSocket with the specified type, tunnel ID, and payload.
    /// </summary>
    /// <param name="ws">The WebSocket to send the frame on.</param>
    /// <param name="type">The frame type (OPEN, DATA, or CLOSE).</param>
    /// <param name="tunnelId">The unique identifier of the tunnel.</param>
    /// <param name="payload">The payload data to send.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    public static async Task SendFrame(
        WebSocket ws,
        byte type,
        Guid tunnelId,
        byte[] payload)
    {
        var buffer = new byte[1 + 16 + payload.Length];
        buffer[0] = type;
        tunnelId.TryWriteBytes(buffer.AsSpan(1, 16));
        payload.CopyTo(buffer.AsSpan(17));

        await ws.SendAsync(
            buffer,
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None
        );
    }

    /// <summary>
    /// Receives a frame from the WebSocket and parses it into type, tunnel ID, and payload.
    /// </summary>
    /// <param name="ws">The WebSocket to receive the frame from.</param>
    /// <returns>A tuple containing the frame type, tunnel ID, and payload.</returns>
    /// <exception cref="WebSocketException">Thrown if the WebSocket is closed.</exception>
    public static async Task<(byte type, Guid tunnelId, byte[] payload)>
        ReceiveFrame(WebSocket ws)
    {
        var ms = new MemoryStream();
        var buffer = new byte[8192];
        WebSocketReceiveResult r;

        do
        {
            r = await ws.ReceiveAsync(buffer, CancellationToken.None);
            if (r.MessageType == WebSocketMessageType.Close)
                throw new WebSocketException("Closed");

            ms.Write(buffer, 0, r.Count);
        }
        while (!r.EndOfMessage);

        var data = ms.ToArray();

        return (
            data[0],
            new Guid(data.AsSpan(1, 16)),
            data[17..]
        );
    }
}
