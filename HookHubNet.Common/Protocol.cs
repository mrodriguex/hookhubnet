using System.Net.WebSockets;

namespace HookHubNet.Common;

public static class TunnelProtocol
{
    public const byte OPEN  = 1;
    public const byte DATA  = 2;
    public const byte CLOSE = 3;

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
