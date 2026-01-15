// using Microsoft.Extensions.Hosting;
// using System.Net;
// using System.Net.Sockets;
// using System.Text;
// using HookHubNet.Common;

// namespace HookHubNet.Hub.Services;

// public class TcpTunnelListener : BackgroundService
// {
//     private readonly TunnelRegistry _registry;

//     public TcpTunnelListener(TunnelRegistry registry)
//     {
//         _registry = registry;
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         var listener = new TcpListener(IPAddress.Any, 8080);
//         listener.Start();

//         Console.WriteLine("Hub listening on :8080");

//         while (!stoppingToken.IsCancellationRequested)
//         {
//             var client = await listener.AcceptTcpClientAsync(stoppingToken);
//             _ = HandleClient(client);
//         }
//     }

//     private async Task HandleClient(TcpClient client)
//     {
//         if (!_registry.Hooks.TryGetValue("default", out var hook))
//         {
//             client.Close();
//             return;
//         }

//         var clientStream = client.GetStream();
//         var tunnelId = Guid.NewGuid();

//         _registry.Tunnels[tunnelId] = clientStream;

//         await TunnelProtocol.SendFrame(
//             hook,
//             TunnelProtocol.OPEN,
//             tunnelId,
//             Array.Empty<byte>()
//         );

//         var buffer = new byte[8192];
//         int read;

//         try
//         {
//             while ((read = await clientStream.ReadAsync(buffer)) > 0)
//             {
//                 await TunnelProtocol.SendFrame(
//                     hook,
//                     TunnelProtocol.DATA,
//                     tunnelId,
//                     buffer[..read]
//                 );
//             }
//         }
//         finally
//         {
//             await TunnelProtocol.SendFrame(
//                 hook,
//                 TunnelProtocol.CLOSE,
//                 tunnelId,
//                 Array.Empty<byte>()
//             );

//             _registry.Tunnels.TryRemove(tunnelId, out _);
//             client.Close();
//         }
//     }
// }
