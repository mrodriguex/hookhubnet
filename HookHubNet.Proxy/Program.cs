using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Configuración de YARP
builder.Services.AddReverseProxy()
    .LoadFromMemory(
        routes: new[]
        {
            // Ruta para tráfico web normal
            new RouteConfig
            {
                RouteId = "web_route",
                ClusterId = "web_cluster",
                Match = new RouteMatch { Path = "/proxy/{**catch-all}" },
                Transforms = new[]
                {
                    new Dictionary<string, string>
                    {
                        { "PathRemovePrefix", "/proxy" } // elimina /proxy antes de enviar al backend
                    }
                }
            },

            // Ruta para tráfico hook (WebSockets)
            new RouteConfig
            {
                RouteId = "hook_route",
                ClusterId = "hook_cluster",
                Match = new RouteMatch { Path = "/hook/{**catch-all}" }
            }
        },
        clusters: new[]
        {
            // Cluster para tráfico web
            new ClusterConfig
            {
                ClusterId = "web_cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "web_destination", new DestinationConfig { Address = "http://localhost:8080/" } }
                }
            },

            // Cluster para tráfico hook (WebSocket)
            new ClusterConfig
            {
                ClusterId = "hook_cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "hook_destination", new DestinationConfig { Address = "http://localhost:5100/" } }
                }
            }
        }
    );

var app = builder.Build();

// 2️⃣ Mapear el proxy
app.MapReverseProxy();

// 3️⃣ Endpoint simple de health check
app.MapGet("/", () => "HookHub Proxy running...");

// 4️⃣ Ejecutar
app.Run();
